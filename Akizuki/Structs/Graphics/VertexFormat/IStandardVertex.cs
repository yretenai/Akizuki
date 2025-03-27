// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Silk.NET.Maths;

namespace Akizuki.Structs.Graphics.VertexFormat;

public interface IStandardVertex {
	public Vector3D<float> Position { get; }
	public Vector3D<float> Normal { get; }
	public Vector2D<Half> UV { get; }
}
