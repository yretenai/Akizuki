// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Text;
using Akizuki.Moo;
using DragonLib;
using DragonLib.IO.Binary;

namespace Akizuki.AssetDb;

public class AssetDatabase : BigWorldFile {
	public AssetDatabase(BufferBinaryReader reader, bool validateChecksum = false) : base(reader, validateChecksum) {
		var headerBaseOffset = reader.Position;
		var header = reader.Read<AssetDatabaseHeader>();

		var stringListBase = headerBaseOffset + AssetDatabaseMap.Size;
		var prototypeTableBase = stringListBase + AssetDatabaseArray.Size;
		var pathListBase = prototypeTableBase + AssetDatabaseMap.Size;

		var stringTableEntries = header.StringTable.Read<StringId, AssetDatabaseMapBucket, int>(headerBaseOffset, reader);
		try {
			foreach (var (key, value) in stringTableEntries) {
				reader.Position = (int) (value + headerBaseOffset + header.StringList.Offset);

				StringId.Lookup[key] = reader.ReadCString<byte>(Encoding.ASCII);
			}
		} finally {
			ObjectPool<Dictionary<StringId, int>>.Return(stringTableEntries);
		}

		PrototypePointers = header.PrototypeTable.Read<ResourceId, AssetDatabaseMapBucket64, AssetTablePrototypePointer>(prototypeTableBase, reader);

		var nameParts = ObjectPool<Dictionary<ResourceId, (string, AssetDatabaseName)>>.Rent();
		nameParts.Clear();
		nameParts.EnsureCapacity((int) header.PathList.Length);
		try {
			using var fileNames = header.PathList.Read<AssetDatabaseName>(pathListBase, reader);
			var nameTableOffset = pathListBase + header.PathList.Offset;
			var oneNameEntry = Unsafe.SizeOf<AssetDatabaseName>();
			foreach (var fileName in fileNames) {
				reader.Position = (int) (nameTableOffset + fileName.Pointer.Offset + 0x10);
				nameParts[fileName.Id] = (reader.ReadCString<byte>(Encoding.ASCII, (int) (fileName.Pointer.Length - 1), true), fileName);
				nameTableOffset += oneNameEntry;
			}

			foreach (var fileName in fileNames) {
				ResolvePath(fileName, nameParts);
			}
		} finally {
			ObjectPool<Dictionary<ResourceId, (string, AssetDatabaseName)>>.Return(nameParts);
		}

		var tableOffset = reader.Position = (int) (headerBaseOffset + header.PrototypeList.Offset);
		Tables.Clear();
		Tables.EnsureCapacity((int) header.PrototypeList.Length);
		using var tableRecords = header.PrototypeList.Read<AssetDatabaseTableHeader>(headerBaseOffset, reader);
		var oneTableSize = Unsafe.SizeOf<AssetDatabaseTableHeader>();

		foreach (var tableRecord in tableRecords) {
			reader.Position = tableOffset + (int) tableRecord.Prototypes.Offset;
			Tables.Add(new AssetDatabaseTable(reader, tableRecord));
			tableOffset += oneTableSize;
		}

		AkizukiLog.Verbose("Loaded {Count} tables", header.PrototypeList.Length);
	}

	public Dictionary<ResourceId, AssetTablePrototypePointer> PrototypePointers { get; set; }
	public List<AssetDatabaseTable> Tables { get; set; } = ObjectPool<List<AssetDatabaseTable>>.Rent();
	public IEnumerable<ResourceId> PresentResources => PrototypePointers.Keys;

	protected override void Dispose(bool disposing) {
		base.Dispose(disposing);
		ObjectPool<Dictionary<ResourceId, AssetTablePrototypePointer>>.Return(PrototypePointers);
		ObjectPool<List<AssetDatabaseTable>>.Return(Tables);
	}

	private static void ResolvePath(AssetDatabaseName fileName, Dictionary<ResourceId, (string Name, AssetDatabaseName FileName)> nameParts) {
		if (ResourceId.Lookup.ContainsKey(fileName.Id)) {
			return;
		}

		ResolvePath(nameParts[fileName.Id].Name, fileName.Id, fileName.ParentId, nameParts);
	}

	private static string ResolvePath(string name, ResourceId id, ResourceId parentId, Dictionary<ResourceId, (string Name, AssetDatabaseName FileName)> names) {
		if (parentId.Hash is 0xDBB1A1D1B108B927ul or 0ul) {
			return name;
		}

		if (ResourceId.Lookup.TryGetValue(parentId, out var parentPath)) {
			return ResourceId.Lookup[id] = parentPath + "/" + name;
		}

		var (parentName, parentFile) = names[parentId];
		return ResourceId.Lookup[id] = ResolvePath(parentName, parentId, parentFile.ParentId, names) + "/" + name;
	}

	public IPrototype? OpenPrototype(ResourceId resource) => throw new NotImplementedException();
}
