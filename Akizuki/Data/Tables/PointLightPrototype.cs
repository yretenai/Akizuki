// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using Akizuki.Structs.Data.Tables;
using DragonLib.IO;
using Silk.NET.Maths;

namespace Akizuki.Data.Tables;

public partial class PointLightPrototype {
	public PointLightPrototype(MemoryReader data) {
		var offset = data.Offset;
		var header = data.Read<PointLightPrototypeHeader>();
		Color = header.Color;
		Position = header.Position;
		Radius = header.Radius;
		MinQuality = header.MinQuality;
		HasAnimatedColor = header.AnimatedColor;
		HasAnimatedRadius = header.AnimatedRadius;

		data.Offset = offset;
		ColorAnimation = new AnimationCurvePrototype<Vector4D<float>>(header.ColorAnimation, data);

		data.Offset = offset + Unsafe.SizeOf<AnimationCurveHeader>();
		RadiusAnimation = new AnimationCurvePrototype<float>(header.RadiusAnimation, data);
	}

	public AnimationCurvePrototype<Vector4D<float>> ColorAnimation { get; set; }
	public AnimationCurvePrototype<float> RadiusAnimation { get; set; }
	public Vector4D<float> Color { get; set; }
	public Vector3D<float> Position { get; set; }
	public float Radius { get; set; }
	public uint MinQuality { get; set; }
	public bool HasAnimatedColor { get; set; }
	public bool HasAnimatedRadius { get; set; }
}
