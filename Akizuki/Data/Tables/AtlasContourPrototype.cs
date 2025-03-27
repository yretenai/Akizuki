// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using DragonLib.IO;
using Silk.NET.Maths;

namespace Akizuki.Data.Tables;

public partial class AtlasContourPrototype : List<List<Vector2D<float>>> {
	public AtlasContourPrototype(MemoryReader data) {
		var offset = data.Offset;
		var info = data.Read<long>(2);
		var count = (int) info[0];
		offset += (int) info[1];

		for (var index = 0; index < count; ++index) {
			data.Offset = offset;
			var pointsInfo = data.Read<long>(2);
			var pointsCount = (int) pointsInfo[0];
			data.Offset = offset + (int) pointsInfo[1];

			var points = new List<Vector2D<float>>();
			points.AddRange(data.Read<Vector2D<float>>(pointsCount));
			Add(points);

			offset += 0x10;
		}
	}
}
