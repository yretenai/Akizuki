// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.IO.MemoryMappedFiles;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Akizuki.Moo;
using Akizuki.PackageFileSystem.V2;
using DragonLib;
using DragonLib.IO.Binary;

namespace Akizuki.PackageFileSystem.V1;

public class PackageV1<TPointer> : Package where TPointer : INumber<TPointer>, IEqualityOperators<TPointer, TPointer, bool> {
	public PackageV1(string basePath, BufferBinaryReader reader, bool validateChecksum = false) : base(reader, validateChecksum) {
		var nameTableOffset = reader.Position;
		var resourceTableOffset = reader.Position;
		var packageTableOffset = reader.Position;

		var indexHeader = reader.Read<PackageIndexHeaderV1<TPointer>>();
		nameTableOffset += int.CreateTruncating(indexHeader.NameTableOffset);
		resourceTableOffset += int.CreateTruncating(indexHeader.ResourceTableOffset);
		packageTableOffset += int.CreateTruncating(indexHeader.StreamTableOffset);

		var nameShift = Unsafe.SizeOf<TPointer>() * 2;
		var oneNameEntry = Unsafe.SizeOf<PackagePathNameV1<TPointer>>();
		var nameParts = ObjectPool<Dictionary<ResourceId, (string, PackagePathNameV1<TPointer>)>>.Rent();
		nameParts.Clear();
		nameParts.EnsureCapacity(indexHeader.NameCount);

		try {
			using var fileNames = reader.Read<PackagePathNameV1<TPointer>>(indexHeader.NameCount);
			foreach (var fileName in fileNames) {
				reader.Position = nameTableOffset + int.CreateTruncating(fileName.Offset) + nameShift;
				nameParts[fileName.Id] = (reader.ReadCString<byte>(Encoding.ASCII, int.CreateTruncating(fileName.Length) - 1, true), fileName);
				nameTableOffset += oneNameEntry;
			}

			foreach (var fileName in fileNames) {
				ResolvePath(fileName, nameParts);
			}
		} finally {
			ObjectPool<Dictionary<ResourceId, (string, PackagePathNameV1<TPointer>)>>.Return(nameParts);
		}

		reader.Position = resourceTableOffset;
		using var resources = reader.Read<PackageResourceV1<TPointer>>(int.CreateTruncating(indexHeader.ResourceCount));
		foreach (var resource in resources) {
			Resources[resource.Id] = resource;
		}

		reader.Position = packageTableOffset;
		oneNameEntry = Unsafe.SizeOf<PackagePathNameV1<TPointer>>();
		using var streams = reader.Read<PackagePathNameV1<TPointer>>(indexHeader.StreamCount);
		foreach (var streamPointer in streams) {
			reader.Position = packageTableOffset + int.CreateTruncating(streamPointer.Offset) + nameShift;
			var name = reader.ReadCString<byte>(Encoding.ASCII, int.CreateTruncating(streamPointer.Length) - 1, true).TrimStart('\\', '/', '.');
			var pkgPath = Path.Combine(basePath, "res_packages", name);
			if (File.Exists(pkgPath)) {
				PackageStreams[streamPointer.Id] = MemoryMappedFile.CreateFromFile(new FileStream(pkgPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), null, 0, MemoryMappedFileAccess.Read, HandleInheritability.Inheritable, false);
			} else {
				AkizukiLog.Warning("Cannot find package data stream {Name}", name);
			}

			packageTableOffset += oneNameEntry;
		}
	}

	public Dictionary<ResourceId, PackageResourceV1<TPointer>> Resources { get; } = [];
	public Dictionary<ResourceId, MemoryMappedFile> PackageStreams { get; } = [];
	public override IEnumerable<ResourceId> PresentResources => Resources.Keys;
	public bool ValidateAssetChecksums { get; set; } = false;

	private static void ResolvePath(PackagePathNameV1<TPointer> fileName, Dictionary<ResourceId, (string Name, PackagePathNameV1<TPointer> FileName)> nameParts) {
		if (ResourceId.Lookup.ContainsKey(fileName.Id)) {
			return;
		}

		ResolvePath(nameParts[fileName.Id].Name, fileName.Id, fileName.ParentId, nameParts);
	}

	private static string ResolvePath(string name, ResourceId id, ResourceId parentId, Dictionary<ResourceId, (string Name, PackagePathNameV1<TPointer> FileName)> names) {
		if (parentId.Hash is 0xDBB1A1D1B108B927ul or 0ul) {
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

		using var accessor = packageStream.CreateViewStream(long.CreateTruncating(resourceHeader.Offset), resourceHeader.CompressedSize, MemoryMappedFileAccess.Read);
		return OpenStreamedResource<TPointer, PackageResourceV1<TPointer>, PackageTileStreamV2<TPointer>>(resource, resourceHeader, accessor, ValidateAssetChecksums);
	}

	protected override void Dispose(bool disposing) {
		foreach (var stream in PackageStreams.Values) {
			stream.Dispose();
		}
	}
}
