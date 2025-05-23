// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Graphics;
using DragonLib.IO;

namespace Akizuki.Graphics;

public sealed class GeometryArmorPlate : IDisposable {
	public GeometryArmorPlate(GeometryArmorPlateHeader header, MemoryReader reader) {
		Thickness = header.Thickness;
		Type = header.Type;
		BoundingBox = header.BoundingBox;
		Vertices = reader.Partition<GeometryArmorVertex>(header.VertexCount, true);
	}

	public int Thickness { get; set; }
	public uint Type { get; set; }
	public BoundingBox BoundingBox { get; set; }
	public IMemoryBuffer<GeometryArmorVertex> Vertices { get; set; }

	public void Dispose() => Vertices.Dispose();
}
