// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Data.Tables;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class AnimationCurvePrototype<T> where T : unmanaged {
	public AnimationCurvePrototype(AnimationCurveHeader header, MemoryReader data) {
		Period = header.Period;
		IsLooping = header.Repeating;

		data.Offset += (int) header.Ramp.PointsPtr + 16;
		Values.AddRange(data.Read<T>(header.Ramp.Count));
	}

	public float Period { get; set; }
	public bool IsLooping { get; set; }
	public List<T> Values { get; set; } = [];
}
