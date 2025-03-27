using System.Runtime.InteropServices;

namespace Akizuki.Structs.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct GeometryVertexBufferHeader {
	public long BufferPtr { get; set; }
	public long VertexFormatLength { get; set; }
	public long VertexFormatPtr { get; set; }
	public int BufferLength { get; set; }
	public ushort VertexStride { get; set; }
	public bool IsSkinned { get; set; }
	public bool HasTangent { get; set; }
}
