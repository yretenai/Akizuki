// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace Akizuki.Structs.Graphics.VertexFormat;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
// ReSharper disable once IdentifierTypo
// ReSharper disable once InconsistentNaming
public record struct VertexFormatXYZNUVIIIWWTB : IBoneVertex, ITangentVertex {
	public Vector3D<float> Position { get; set; }
	public Vector4D<sbyte> PackedNormal { get; set; }
	public Vector2D<Half> PackedUV { get; set; }
	public Vector4D<byte> PackedBoneIndex { get; set; }
	public Vector4D<byte> PackedBoneWeight { get; set; }
	public Vector4D<sbyte> PackedTangent { get; set; }
	public Vector4D<sbyte> PackedBinormal { get; set; }

	public Vector3D<float> Normal => VertexHelper.UnpackNormal(PackedNormal);
	public Vector4D<byte> BoneIndex => VertexHelper.UnpackBoneIndex(PackedBoneIndex);
	public Vector4D<float> BoneWeight => VertexHelper.UnpackBoneWeight(PackedBoneWeight);
	public Vector3D<float> Tangent => VertexHelper.UnpackNormal(PackedTangent);
	public Vector3D<float> Binormal => VertexHelper.UnpackNormal(PackedBinormal);
	public Vector2D<float> UV => VertexHelper.UnpackUV(PackedUV);

	public static VertexInfo VertexInfo => new() {
		BoneIndex = 20,
		BoneWeight = 24,
		Tangent = 28,
		Binormal = 32,
	};
}
