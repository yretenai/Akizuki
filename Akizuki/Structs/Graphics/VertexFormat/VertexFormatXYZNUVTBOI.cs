// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace Akizuki.Structs.Graphics.VertexFormat;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
// ReSharper disable once IdentifierTypo
// ReSharper disable once InconsistentNaming
public record struct VertexFormatXYZNUVTBOI : ITangentVertex, IColorVertex {
	public Vector3D<float> Position { get; set; }
	public Vector4D<sbyte> PackedNormal { get; set; }
	public Vector2D<Half> PackedUV { get; set; }
	public Vector4D<sbyte> PackedTangent { get; set; }
	public Vector4D<sbyte> PackedBinormal { get; set; }
	public Vector4D<byte> PackedColor { get; set; }

	public Vector3D<float> Normal => VertexHelper.UnpackNormal(PackedNormal);
	public Vector3D<float> Tangent => VertexHelper.UnpackNormal(PackedTangent);
	public Vector3D<float> Binormal => VertexHelper.UnpackNormal(PackedBinormal);
	public Vector4D<float> Color => VertexHelper.UnpackColor(PackedColor);
	public Vector2D<float> UV => VertexHelper.UnpackUV(PackedUV);
	public static VertexInfo VertexInfo => new() {
		Tangent = 20,
		Binormal = 24,
		Color = 28,
	};
}
