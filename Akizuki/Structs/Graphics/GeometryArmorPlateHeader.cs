// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct GeometryArmorPlateHeader {
	public ushort Thickness { get; set; }
	public ushort Type { get; set; }
	public BoundingBox BoundingBox { get; set; }
	public int VertexCount { get; set; }
}
