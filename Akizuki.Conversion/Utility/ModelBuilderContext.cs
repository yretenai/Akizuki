using Akizuki.Structs.Data;
using GeometryCache = System.Collections.Generic.Dictionary<(bool IsVertexBuffer, int GeometryBufferId), System.Collections.Generic.Dictionary<string, int>>;
using PrimitiveCache = System.Collections.Generic.Dictionary<(Akizuki.Structs.Graphics.GeometryName Vertex, Akizuki.Structs.Graphics.GeometryName Index), (System.Collections.Generic.Dictionary<string, int> Attributes, int Indices)>;

namespace Akizuki.Conversion.Utility;

public record ModelBuilderContext(
	IConversionOptions Flags,
	ResourceManager Manager,
	Stream BufferStream,
	string ModelPath,
	string TexturesPath,
	Dictionary<string, HashSet<string>> HardPoints,
	Dictionary<string, string> PortPoints) {
	public Dictionary<int, int> ThicknessMaterialCache { get; } = [];
	public Dictionary<ResourceId, PrimitiveCache> PrimCache { get; } = [];
	public Dictionary<ResourceId, GeometryCache> GeometryCache { get; } = [];
	public Dictionary<ResourceId, int> MaterialCache { get; } = [];
	public Dictionary<ResourceId, int> TextureCache { get; } = [];
}
