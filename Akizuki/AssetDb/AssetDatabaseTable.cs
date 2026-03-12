// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Moo;
using DragonLib.IO.Binary;

namespace Akizuki.AssetDb;

public class AssetDatabaseTable {
	private static readonly Dictionary<uint, LoadPrototypeDelegate> Prototypes = new();

	public AssetDatabaseTable(BufferBinaryReader data, AssetDatabaseTableHeader header) {
		Id = header.Id;
		Version = header.Version;
		var pos = data.Position;
		var info = data.Read<AssetDatabaseArray>();
		var count = info.Length;
		var offset = pos + info.Offset;
		AkizukiLog.Debug("Creating Table {Table} ({Version:x8})", Id.ToDebugString(), Version);

		if (Prototypes.TryGetValue(Id, out var impl)) {
			impl(this, data, (int) count, (int) offset);
		} else {
			AkizukiLog.Warning("{Id} does not have an implementation", Id.ToDebugString());
		}
	}

	public StringId Id { get; set; }
	public uint Version { get; set; }
	public List<IPrototype> Records { get; set; } = [];

	private static void CheckRecords<T>(AssetDatabaseTable table, BufferBinaryReader data, int count, int offset) where T : IPrototype {
		if (T.Size == 0) {
			AkizukiLog.Warning("{Name} does not have an implementation", T.PrototypeName);
			return;
		}

		if (table.Version != T.Version) {
			AkizukiLog.Warning("Tried loading {Name} with an unsupported version!", T.PrototypeName);
			return;
		}

		AkizukiLog.Debug("{Name} Version Matches", T.PrototypeName);
		AkizukiLog.Debug("Creating Records for {Name}", T.PrototypeName);

		table.CreateRecords<T>(data, count, offset);
	}

	private void CreateRecords<T>(BufferBinaryReader data, int count, int offset) where T : IPrototype {
		for (var index = 0; index < count; ++index) {
			data.Position = offset;
			Records.Add(T.Create(data));
			offset += T.Size;
		}
	}

	private delegate void LoadPrototypeDelegate(AssetDatabaseTable table, BufferBinaryReader data, int count, int offset);
}
