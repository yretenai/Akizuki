// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Silk.NET.Maths;

namespace Akizuki.Structs.Graphics.VertexFormat;

public interface IUV2Vertex : IStandardVertex {
	public Vector2D<float> UV2 { get; }
}
