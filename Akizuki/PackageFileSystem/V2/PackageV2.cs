// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Akizuki.Moo;
using DragonLib;
using DragonLib.IO.Binary;
using Waterfall.Compression;
using Waterfall.Hash;
using Waterfall.Hash.Basis;

namespace Akizuki.PackageFileSystem.V2;

public class PackageV2<TPointer> : Package where TPointer : INumber<TPointer>, IEqualityOperators<TPointer, TPointer, bool> {
	public PackageV2(string basePath, BufferBinaryReader reader, bool validateChecksum = false) : base(reader, validateChecksum) {
		var nameTableOffset = reader.Position;
		var resourceTableOffset = reader.Position;
		var packageTableOffset = reader.Position;

		var indexHeader = reader.Read<PackageIndexHeaderV2<TPointer>>();
		nameTableOffset += int.CreateTruncating(indexHeader.NameTableOffset);
		resourceTableOffset += int.CreateTruncating(indexHeader.ResourceTableOffset);
		packageTableOffset += int.CreateTruncating(indexHeader.StreamTableOffset);

		var oneNameEntry = Unsafe.SizeOf<PackagePathNameV2<TPointer>>();
		var nameParts = ObjectPool<Dictionary<ResourceId, (string, PackagePathNameV2<TPointer>)>>.Rent();
		nameParts.EnsureCapacity(indexHeader.NameCount);

		try {
			using var fileNames = reader.Read<PackagePathNameV2<TPointer>>(indexHeader.NameCount);
			foreach (var fileName in fileNames) {
				reader.Position = nameTableOffset + int.CreateTruncating(fileName.Name.Offset);
				nameParts[fileName.Name.Id] = (reader.ReadCString<byte>(Encoding.ASCII, int.CreateTruncating(fileName.Name.Length) - 1, true), fileName);
				nameTableOffset += oneNameEntry;
			}

			foreach (var fileName in fileNames) {
				ResolvePath(fileName, nameParts);
			}
		} finally {
			ObjectPool<Dictionary<ResourceId, (string, PackagePathNameV2<TPointer>)>>.Return(nameParts);
		}

		reader.Position = resourceTableOffset;
		using var resources = reader.Read<PackageResourceV2<TPointer>>(int.CreateTruncating(indexHeader.ResourceCount));
		foreach (var resource in resources) {
			Resources[resource.Id] = resource;
		}

		reader.Position = packageTableOffset;
		oneNameEntry = Unsafe.SizeOf<PackageResourceNameV2<TPointer>>();
		using var streams = reader.Read<PackageResourceNameV2<TPointer>>(indexHeader.StreamCount);
		foreach (var streamPointer in streams) {
			reader.Position = packageTableOffset + int.CreateTruncating(streamPointer.Offset);
			var name = reader.ReadCString<byte>(Encoding.ASCII, int.CreateTruncating(streamPointer.Length) - 1, true);
			var pkgPath = Path.Combine(basePath, "res_packages", name);
			if (File.Exists(pkgPath)) {
				PackageStreams[streamPointer.Id] = MemoryMappedFile.CreateFromFile(new FileStream(pkgPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), null, 0, MemoryMappedFileAccess.Read, HandleInheritability.Inheritable, false);
			} else {
				AkizukiLog.Warning("Cannot find package data stream {Name}", name);
			}

			packageTableOffset += oneNameEntry;
		}
	}

	public Dictionary<ResourceId, PackageResourceV2<TPointer>> Resources { get; } = [];
	public Dictionary<ResourceId, MemoryMappedFile> PackageStreams { get; } = [];
	public override IEnumerable<ResourceId> PresentResources => Resources.Keys;
	public bool ValidateAssetChecksums { get; set; } = false;

	private static void ResolvePath(PackagePathNameV2<TPointer> fileName, Dictionary<ResourceId, (string Name, PackagePathNameV2<TPointer> FileName)> nameParts) {
		if (ResourceId.Lookup.ContainsKey(fileName.Name.Id)) {
			return;
		}

		ResolvePath(nameParts[fileName.Name.Id].Name, fileName.Name.Id, fileName.ParentId, nameParts);
	}

	private static string ResolvePath(string name, ResourceId id, ResourceId parentId, Dictionary<ResourceId, (string Name, PackagePathNameV2<TPointer> FileName)> names) {
		if (parentId == 0xDBB1A1D1B108B927ul) {
			return name;
		}

		if (ResourceId.Lookup.TryGetValue(parentId, out var parentPath)) {
			return ResourceId.Lookup[id] = parentPath + "/" + name;
		}

		var (parentName, parentFile) = names[parentId];
		return ResourceId.Lookup[id] = ResolvePath(parentName, parentId, parentFile.ParentId, names) + "/" + name;
	}

	public override RentedArray<byte>? OpenResource(ResourceId resource) {
		if (!Resources.TryGetValue(resource, out var resourceHeader)) {
			AkizukiLog.Debug("Could not find asset {Package}", resource.ToDebugString());
			return default;
		}

		if (!PackageStreams.TryGetValue(resourceHeader.StreamId, out var packageStream)) {
			AkizukiLog.Debug("Could not find stream for {Package}", resource.ToDebugString());
			return default;
		}

		var data = new RentedArray<byte>(int.CreateChecked(resourceHeader.Size));
		var dataSpan = data.Span;
		using var accessor = packageStream.CreateViewStream(long.CreateTruncating(resourceHeader.Offset), resourceHeader.CompressedSize, MemoryMappedFileAccess.Read);

		try {
			if (resourceHeader.CompressionType == PackageCompressionType.None || resourceHeader.CompressionLevel == 0) {
				Debug.Assert(TPointer.CreateChecked(resourceHeader.CompressedSize) == resourceHeader.Size);
				accessor.ReadExactly(dataSpan);
			} else {
				using var compressed = new RentedArray<byte>(resourceHeader.CompressedSize);
				var dataMemory = data.Memory;
				var compressedMemory = compressed.Memory;
				var compressedSpan = compressed.Span;
				accessor.ReadExactly(compressedSpan);

				switch (resourceHeader.CompressionType) {
					case PackageCompressionType.None: throw new UnreachableException();
					case PackageCompressionType.DeflateBlocks: throw new NotImplementedException("DeflateBlocks compression hasn't been seen yet.");
					case PackageCompressionType.Deflate: {
						var n = CompressionHelper.Decompress(CompressionType.Deflate, compressedMemory, dataMemory);
						Debug.Assert(n == data.Length);
						break;
					}
					case <= PackageCompressionType.OodleHydra: {
						var streamHeader = MemoryMarshal.Read<PackageDataStreamHeaderV2<TPointer>>(compressedMemory.Span);
						var remainingSize = long.CreateChecked(streamHeader.Size);
						var totalSize = remainingSize;
						var offset = int.CreateChecked(streamHeader.DataOffset) + 8;
						var blocks = MemoryMarshal.Cast<byte, int>(compressedMemory.Span[Unsafe.SizeOf<PackageDataStreamHeaderV2<TPointer>>()..])[..streamHeader.BlockCount];
						Debug.Assert(streamHeader.Size == resourceHeader.Size);
						foreach (var block in blocks) {
							var start = int.CreateChecked(totalSize - remainingSize);
							var size = Math.Min(remainingSize, streamHeader.BlockSize);
							var end = int.CreateChecked(start + Math.Min(remainingSize, streamHeader.BlockSize));
							var n = CompressionHelper.Decompress(CompressionType.Oodle, compressedMemory[offset..(offset + block)], dataMemory[start..end]);
							Debug.Assert(n == size);
							offset += block;
							remainingSize -= size;
						}

						break;
					}
					default: throw new NotSupportedException($"compression type {resourceHeader.CompressionType} is not supported");
				}
			}

			if (ValidateAssetChecksums) {
				var hash = CRC.HashData(CRC32Variants.ISO, dataSpan);
				if (hash != resourceHeader.Checksum) {
					throw new InvalidDataException("Checksum mismatch");
				}

				AkizukiLog.Debug("{File} Passed Checksum Validation", resource.ToDebugString());
			}
		} catch {
			data.Dispose();
			throw;
		}

		return data;
	}

	protected override void Dispose(bool disposing) {
		foreach (var stream in PackageStreams.Values) {
			stream.Dispose();
		}
	}
}
