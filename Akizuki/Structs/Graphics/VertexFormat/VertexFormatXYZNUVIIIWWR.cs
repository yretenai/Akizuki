// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace Akizuki.Structs.Graphics.VertexFormat;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
// ReSharper disable once IdentifierTypo
// ReSharper disable once InconsistentNaming
public record struct VertexFormatXYZNUVIIIWWR : IBoneVertex, IRadiusVertex {
	public Vector3D<float> Position { get; set; }
	public Vector4D<sbyte> PackedNormal { get; set; }
	public Vector2D<Half> PackedUV { get; set; }
	public Vector4D<byte> BoneIndex { get; set; }
	public Vector4D<byte> PackedBoneWeight { get; set; }
	public float Radius { get; set; }

	public Vector3D<float> Normal => VertexHelper.Unpack(PackedNormal);
	public Vector4D<float> BoneWeight => VertexHelper.Unpack(PackedBoneWeight);
	public Vector2D<float> UV => VertexHelper.Unpack(PackedUV);
}
