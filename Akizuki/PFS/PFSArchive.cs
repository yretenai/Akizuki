// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Akizuki.Exceptions;
using Akizuki.Structs.PFS;
using DragonLib.Hash;
using DragonLib.Hash.Algorithms;
using DragonLib.Hash.Basis;
using DragonLib.IO;
using Waterfall.Compression;

namespace Akizuki.PFS;

public sealed class PFSArchive : IDisposable {
	public PFSArchive(string packageDirectory, string path, bool validate = true) :
		this(packageDirectory, new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), false, validate) { }

	public PFSArchive(string packageDirectory, Stream stream, bool leaveOpen = false, bool validate = true) {
		ShouldValidate = validate;

		using var deferred = new DeferredDisposable(() => {
			if (!leaveOpen) {
				stream.Dispose();
			}
		});

		using var rented = stream.ToRented();
		var data = new SpanReader(rented.Span);
		Header = data.Read<PFSIndexHeader>();
		if (Header.EndianTest is not 0x02000000) {
			throw new NotSupportedException("PFS Index Big Endian is not supported");
		}

		if (Header.Magic != PFSIndexHeader.PFSIMagic) {
			throw new InvalidDataException("File is not recognised as a PFS Index");
		}

		if (Header.Bits is not 64) {
			throw new NotSupportedException($"PFS Index Bit Size of {Header.Bits} not supported");
		}

		if (ShouldValidate) {
			var hash = MurmurHash3Algorithm.Hash32_32(data.Buffer[0x10..]);
			if (hash != Header.Hash) {
				throw new CorruptDataException("PFS Index Checksum Mismatch");
			}

			AkizukiLog.Verbose("PFS Passed Checksum Validation");
		}

		AkizukiLog.Debug("{Value}", Header);

		var pfsOffset = data.Offset;
		Info = data.Read<PFSIndexInfo>();
		AkizukiLog.Debug("{Value}", Info);

		var nameOffset = pfsOffset + (int) Info.FileNameSectionPtr;
		data.Offset = nameOffset;
		var oneNameEntry = Unsafe.SizeOf<PFSFileName>();

		var fileNames = data.Read<PFSFileName>(Info.FileNameCount);
		var names = new Dictionary<ulong, (string, PFSFileName)>(Info.FileNameCount);
		foreach (var fileName in fileNames) {
			data.Offset = nameOffset + (int) fileName.Name.NamePtr;
			names[fileName.Name.Id] = (data.ReadString((int) fileName.Name.NameLength - 1), fileName);
			nameOffset += oneNameEntry;
		}

		Paths.EnsureCapacity(Info.FileNameCount);
		foreach (var fileName in fileNames) {
			ResolvePath(fileName, names);
		}

		data.Offset = pfsOffset + (int) Info.FileInfoSectionPtr;
		var files = data.Read<PFSFile>(Info.FileInfoCount);
		Files.EnsureCapacity(Info.FileNameCount);
		foreach (var file in files) {
			Files.Add(file);
		}

		Files.Sort();

		var packageOffset = pfsOffset + (int) Info.PackageSectionPtr;
		data.Offset = packageOffset;
		var onePkgEntry = Unsafe.SizeOf<PFSNamedId>();
		var packageNames = data.Read<PFSNamedId>(Info.PackageCount);
		foreach (var packageName in packageNames) {
			data.Offset = packageOffset + (int) packageName.NamePtr;
			var path = Path.Combine(packageDirectory, data.ReadString((int) packageName.NameLength - 1));

			if (File.Exists(path)) {
				Packages[packageName.Id] = new FileStream(Path.Combine(packageDirectory, path), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			}

			packageOffset += onePkgEntry;
		}
	}

	public PFSIndexHeader Header { get; }
	public PFSIndexInfo Info { get; }

	public Dictionary<ulong, string> Paths { get; } = new() {
		[0xDBB1A1D1B108B927ul] = "res",
	};

	public List<PFSFile> Files { get; } = [];
	public Dictionary<ulong, Stream> Packages { get; } = [];
	public bool ShouldValidate { get; }

	public void Dispose() {
		foreach (var (_, stream) in Packages) {
			stream.Dispose();
		}
	}

	private void ResolvePath(PFSFileName fileName, Dictionary<ulong, (string Name, PFSFileName FileName)> names) {
		if (Paths.ContainsKey(fileName.Name.Id)) {
			return;
		}

		ResolvePath(names[fileName.Name.Id].Name, fileName.Name.Id, fileName.ParentId, names);
	}

	private string ResolvePath(string name, ulong id, ulong parentId, Dictionary<ulong, (string Name, PFSFileName FileName)> names) {
		if (Paths.TryGetValue(parentId, out var parentPath)) {
			return Paths[id] = parentPath + "/" + name;
		}

		var (parentName, parentFile) = names[parentId];
		return Paths[id] = ResolvePath(parentName, parentId, parentFile.ParentId, names) + "/" + name;
	}

	public IMemoryBuffer<byte>? OpenFile(string path) {
		foreach (var (id, name) in Paths) {
			if (name.Equals(path, StringComparison.OrdinalIgnoreCase)) {
				return OpenFile(id);
			}
		}

		return null;
	}

	public IMemoryBuffer<byte>? OpenFile(ulong id) {
		var fileIndex = Files.BinarySearch(new PFSFile { Id = id });
		if (fileIndex < 0 || fileIndex >= Files.Count) {
			return null;
		}

		return OpenFile(Files[fileIndex]);
	}

	public IMemoryBuffer<byte>? OpenFile(PFSFile file) {
		if (!Packages.TryGetValue(file.PackageId, out var packageStream)) {
			return null;
		}

		var data = new MemoryBuffer<byte>(int.CreateChecked(file.UncompressedSize));
		var dataMemory = data.Memory;
		var dataSpan = data.Span;

		try {
			if ((file.Flags & PFSFileFlags.Compressed) != 0) {
				using var compressed = new MemoryBuffer<byte>(int.CreateChecked(file.CompressedSize));
				var compressedMemory = compressed.Memory;
				var compressedSpan = compressed.Span;

				packageStream.Position = file.Offset;
				packageStream.ReadExactly(compressedSpan);

				switch (file.CompressionType) {
					case PFSCompressionType.None:
						throw new UnreachableException();
					case PFSCompressionType.Deflate:
						CompressionHelper.Decompress(CompressionType.Deflate, compressedMemory, dataMemory);
						break;
					default: throw new NotSupportedException();
				}
			} else {
				packageStream.Position = file.Offset;
				packageStream.ReadExactly(dataSpan);
			}

			if (ShouldValidate) {
				var hash = CRC.HashData(CRC32Variants.ISO, dataSpan);
				if (hash != file.Hash) {
					throw new InvalidDataException("Checksum mismatch");
				}

				AkizukiLog.Verbose("File {File:x16} Passed Checksum Validation", file.Id);
			}
		} catch {
			data.Dispose();
			throw;
		}

		return data;
	}
}
