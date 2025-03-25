// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace Akizuki.Structs.Data.Tables;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x70)]
public record struct PointLightPrototypeHeader {
	[field: FieldOffset(0)]
	public AnimationCurveHeader ColorAnimation { get; set; }

	[field: FieldOffset(32)]
	public AnimationCurveHeader RadiusAnimation { get; set; }

	[field: FieldOffset(64)]
	public Vector4D<float> Color { get; set; }

	[field: FieldOffset(80)]
	public Vector3D<float> Position { get; set; }

	[field: FieldOffset(96)]
	public float Radius { get; set; }

	[field: FieldOffset(100)]
	public uint MinQuality { get; set; }

	[field: FieldOffset(104)]
	public bool AnimatedColor { get; set; }

	[field: FieldOffset(105)]
	public bool AnimatedRadius { get; set; }
}
