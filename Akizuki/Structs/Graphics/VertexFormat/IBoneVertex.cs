// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Silk.NET.Maths;

namespace Akizuki.Structs.Graphics.VertexFormat;

public interface IBoneVertex {
	public Vector4D<byte> BoneIndex { get; }
	public Vector4D<float> BoneWeight { get; }
}
