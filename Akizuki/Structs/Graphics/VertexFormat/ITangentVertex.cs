// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Silk.NET.Maths;

namespace Akizuki.Structs.Graphics.VertexFormat;

public interface ITangentVertex {
	public Vector3D<float> Tangent { get; }
	public Vector3D<float> Binormal { get; }
}
