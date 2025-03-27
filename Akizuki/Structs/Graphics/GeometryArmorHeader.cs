using System.Runtime.InteropServices;

namespace Akizuki.Structs.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct GeometryArmorHeader {
	public uint Id { get; set; }
	public BoundingBox BoundingBox { get; set; }
	public int PlateCount { get; set; }
}
