// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Data.Tables;
using Akizuki.Structs.Data;
using DragonLib.IO;

namespace Akizuki.Data;

public class BigWorldTable {
	private static readonly Dictionary<uint, LoadPrototypeDelegate> Prototypes = new() {
		[MaterialPrototype.Id] = CheckRecords<MaterialPrototype>,
		[VisualPrototype.Id] = CheckRecords<VisualPrototype>,
		[ModelPrototype.Id] = CheckRecords<ModelPrototype>,
		[PointLightPrototype.Id] = CheckRecords<PointLightPrototype>,
		[VelocityFieldPrototype.Id] = CheckRecords<VelocityFieldPrototype>,
		[AtlasContourPrototype.Id] = CheckRecords<AtlasContourPrototype>,
		[EffectPrototype.Id] = CheckRecords<EffectPrototype>,
		[EffectPresetPrototype.Id] = CheckRecords<EffectPresetPrototype>,
		[EffectMetadataPrototype.Id] = CheckRecords<EffectMetadataPrototype>,
	};

	public BigWorldTable(MemoryReader data, BWDBTableHeader header, BigWorldDatabase db) {
		Id = header.Id;
		Version = header.Hash;

		var info = data.Read<long>(2);
		var count = (int) info[0];
		var offset = (int) info[1];
		AkizukiLog.Debug("Creating Table {Table:x8} ({Version:x8})", Id, Version);

		if (Prototypes.TryGetValue(Id, out var impl)) {
			impl(this, data, count, offset, db);
		} else {
			AkizukiLog.Warning("{Id:x8} does not have an implementation", Id);
		}
	}

	public uint Id { get; set; }
	public uint Version { get; set; }
	public List<IPrototype> Records { get; set; } = [];

	private static void CheckRecords<T>(BigWorldTable table, MemoryReader data, int count, int offset, BigWorldDatabase db) where T : IPrototype {
		if (T.Size == 0) {
			AkizukiLog.Warning("{Name} does not have an implementation", T.PrototypeName);
			return;
		}

		if (table.Version != T.Version) {
			AkizukiLog.Warning("Tried loading {Name} with an unsupported version!", T.PrototypeName);
		} else {
			AkizukiLog.Debug("{Name} Version Matches", T.PrototypeName);
		}

		AkizukiLog.Debug("Creating Records for {Name}", T.PrototypeName);
		table.CreateRecords<T>(data, count, offset);
	}

	private void CreateRecords<T>(MemoryReader data, int count, int offset) where T : IPrototype {
		for (var index = 0; index < count; ++index) {
			data.Offset = offset;
			Records.Add(T.Create(data));
			offset += T.Size;
		}
	}

	private delegate void LoadPrototypeDelegate(BigWorldTable table, MemoryReader data, int count, int offset, BigWorldDatabase db);
}
