// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Akizuki.Structs.Graphics.VertexFormat;
using Silk.NET.Maths;

namespace Akizuki.Structs.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct GeometryArmorVertex {
	public Vector3D<float> Position { get; set; }
	public Vector4D<sbyte> PackedNormal { get; set; }
	public Vector3D<float> Normal => VertexHelper.UnpackNormal(PackedNormal);
}
