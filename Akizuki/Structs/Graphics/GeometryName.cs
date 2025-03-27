using System.Runtime.InteropServices;
using Akizuki.Structs.Data;

namespace Akizuki.Structs.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct GeometryName {
	public StringId Name { get; set; }
	public ushort BufferIndex { get; set; }
	public ushort MeshId { get; set; }
	public int BufferOffset { get; set; }
	public int BufferLength { get; set; }
}
