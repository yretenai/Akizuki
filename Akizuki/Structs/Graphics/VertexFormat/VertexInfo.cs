// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace Akizuki.Structs.Graphics.VertexFormat;

public readonly record struct VertexInfo() {
	public int Position { get; init; } = 0;
	public int Normal { get; init; } = 12;
	public int UV1 { get; init; } = 16;
	public int UV2 { get; init; } = -1;
	public int BoneIndex { get; init; } = -1;
	public int BoneWeight { get; init; } = -1;
	public int Tangent { get; init; } = -1;
	public int Binormal { get; init; } = -1;
	public int Radius { get; init; } = -1;
	public int Color { get; init; } = -1;
	public int Id { get; init; } = -1;
}
