using System.Runtime.InteropServices;

namespace Akizuki.Structs.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct GeometryIndexBufferHeader {
	public long BufferPtr { get; set; }
	public int BufferLength { get; set; }
	public ushort Unknown { get; set; }
	public short IndexStride { get; set; }
}
