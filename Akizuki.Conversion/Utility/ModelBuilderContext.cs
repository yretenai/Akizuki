// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Data;
using Akizuki.Structs.Data.Camouflage;
using GeometryCache = System.Collections.Generic.Dictionary<(bool IsVertexBuffer, int GeometryBufferId), System.Collections.Generic.Dictionary<string, int>>;
using PrimitiveCache = System.Collections.Generic.Dictionary<(Akizuki.Structs.Graphics.GeometryName Vertex, Akizuki.Structs.Graphics.GeometryName Index), (System.Collections.Generic.Dictionary<string, int> Attributes, int Indices)>;

namespace Akizuki.Conversion.Utility;

public record ModelBuilderContext(
	IConversionOptions Flags,
	ResourceManager Manager,
	Stream BufferStream,
	bool IsEvent,
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

public record CamouflageContext(CamouflageColorScheme? ColorScheme, Camouflage Camouflage, CamouflagePart Part, Dictionary<string, string> Redirect, HashSet<string> MiscFilter, List<string> Style) {
	public bool SkipFilters => MiscFilter.Count > 0 || Style.Count > 0;
}
