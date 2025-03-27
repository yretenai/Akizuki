// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Data;
using Akizuki.Structs.Data.Tables;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class DyePrototype {
	public DyePrototype(DyePrototypeHeader header, MemoryReader data) {
		var offset = data.Offset;

		Matter = header.MatterId;
		Replaces = header.ReplacesId;

		data.Offset = (int) (offset + header.TintNameIdsPtr);
		var names = data.Read<StringId>(header.TintCount);
		for (var index = 0; index < header.TintCount; ++index) {
			Tints.Add(names[index]);
		}

		data.Offset = (int) (offset + header.TintMaterialIdsPtr);
		var ids = data.Read<ResourceId>(header.TintCount);
		for (var index = 0; index < header.TintCount; ++index) {
			Materials.Add(ids[index]);
		}
	}

	public StringId Matter { get; set; }
	public StringId Replaces { get; set; }
	public List<StringId> Tints { get; set; } = [];
	public List<ResourceId> Materials { get; set; } = [];
}
