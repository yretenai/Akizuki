// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Akizuki.Exceptions;
using Akizuki.Structs.Data;
using DragonLib.Hash;
using DragonLib.Hash.Algorithms;
using DragonLib.Hash.Basis;
using DragonLib.IO;
using Waterfall.Compression;

namespace Akizuki.Data;

public sealed class PackageFileSystem : IDisposable {
	public PackageFileSystem(string packageDirectory, string path, bool validate = true) :
		this(packageDirectory, new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), false, validate) { }

	public PackageFileSystem(string packageDirectory, Stream stream, bool leaveOpen = false, bool validate = true) {
		ShouldValidate = validate;

		using var deferred = new DeferredDisposable(() => {
			if (!leaveOpen) {
				stream.Dispose();
			}
		});

		using var rented = stream.ToRented();
		var data = new SpanReader(rented.Span);
		BigWorldHeader = data.Read<BWFileHeader>();

		if (!BigWorldHeader.IsHostEndian) {
			throw new NotSupportedException("PFS Index Big Endian is not supported");
		}

		if (BigWorldHeader.Version != 2) {
			throw new NotSupportedException("Only PFS Version 2 is supported");
		}

		if (BigWorldHeader.Magic != BWFileHeader.PFSIMagic) {
			throw new InvalidDataException("File is not recognised as a PFS Index");
		}

		if (BigWorldHeader.PointerSize is not 64) {
			throw new NotSupportedException($"PFS Index Bit Size of {BigWorldHeader.PointerSize} not supported");
		}

		var baseRel = Unsafe.SizeOf<BWFileHeader>();

		if (ShouldValidate) {
			var hash = MurmurHash3Algorithm.Hash32_32(data.Buffer[baseRel..]);
			if (hash != BigWorldHeader.Hash) {
				throw new CorruptDataException("PFS Index Checksum Mismatch");
			}

			AkizukiLog.Verbose("PFS Passed Checksum Validation");
		}

		AkizukiLog.Debug("{Value}", BigWorldHeader);

		Header = data.Read<PFSIndexHeader>();
		AkizukiLog.Debug("{Value}", Header);

		var nameOffset = baseRel + (int) Header.FileNameSectionPtr;
		data.Offset = nameOffset;
		var oneNameEntry = Unsafe.SizeOf<PFSFileName>();

		var fileNames = data.Read<PFSFileName>(Header.FileNameCount);
		var names = new Dictionary<ulong, (string, PFSFileName)>(Header.FileNameCount);
		foreach (var fileName in fileNames) {
			data.Offset = nameOffset + (int) fileName.Name.NamePtr;
			names[fileName.Name.Id] = (data.ReadString((int) fileName.Name.NameLength - 1), fileName);
			nameOffset += oneNameEntry;
		}

		Paths.EnsureCapacity(Header.FileNameCount);
		foreach (var fileName in fileNames) {
			ResolvePath(fileName, names);
		}

		data.Offset = baseRel + (int) Header.FileInfoSectionPtr;
		var files = data.Read<PFSFile>(Header.FileInfoCount);
		Files.EnsureCapacity(Header.FileNameCount);
		foreach (var file in files) {
			Files.Add(file);
		}

		Files.Sort();

		var packageOffset = baseRel + (int) Header.PackageSectionPtr;
		data.Offset = packageOffset;
		var onePkgEntry = Unsafe.SizeOf<PFSNamedId>();
		var packageNames = data.Read<PFSNamedId>(Header.PackageCount);
		foreach (var packageName in packageNames) {
			data.Offset = packageOffset + (int) packageName.NamePtr;
			var path = Path.Combine(packageDirectory, data.ReadString((int) packageName.NameLength - 1));

			if (File.Exists(path)) {
				Packages[packageName.Id] = new FileStream(Path.Combine(packageDirectory, path), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			}

			packageOffset += onePkgEntry;
		}
	}

	public BWFileHeader BigWorldHeader { get; }
	public PFSIndexHeader Header { get; }

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
