// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Data;
using Akizuki.Structs.Data.Tables;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class LODPrototype {
	public LODPrototype(LODPrototypeHeader header, MemoryReader data) {
		Extent = header.Extent;
		CastsShadows = header.CastsShadows;

		data.Offset += (int) header.RenderSetNamesPtr;
		var names = data.Read<StringId>(header.RenderSetNamesCount);
		RenderSets.AddRange(names);
	}

	public float Extent { get; set; }
	public bool CastsShadows { get; set; }
	public List<StringId> RenderSets { get; set; } = [];
}
