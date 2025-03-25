// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Data.Tables;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class LODPrototype {
	public LODPrototype(LODPrototypeHeader header, MemoryReader data, BigWorldDatabase db) {
		Extent = header.Extent;
		CastsShadows = header.CastsShadows;

		data.Offset += (int) header.RenderSetNamesPtr;
		var names = data.Read<uint>(header.RenderSetNamesCount);
		for (var index = 0; index < header.RenderSetNamesCount; ++index) {
			RenderSets.Add(db.GetString(names[index]));
		}
	}

	public float Extent { get; set; }
	public bool CastsShadows { get; set; }
	public List<string> RenderSets { get; set; } = [];
}
