// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct GeometryHeader {
	public int VertexBufferCount { get; set; }
	public int IndexBufferCount { get; set; }
	public int VertexNameCount { get; set; }
	public int IndexNameCount { get; set; }
	public int CollisionCount { get; set; }
	public int ArmorBufferCount { get; set; }
	public long VertexNamesPtr { get; set; }
	public long IndexNamesPtr { get; set; }
	public long VertexBuffersPtr { get; set; }
	public long IndexBuffersPtr { get; set; }
	public long CollisionBuffersPtr { get; set; }
	public long ArmorBuffersPtr { get; set; }
}
