// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Data;
using Akizuki.Structs.Data.Camouflage;
using GeometryCache = System.Collections.Generic.Dictionary<(bool IsVertexBuffer, int GeometryBufferId), System.Collections.Generic.Dictionary<string, int>>;
using PrimitiveCache = System.Collections.Generic.Dictionary<(Akizuki.Structs.Graphics.GeometryName Vertex, Akizuki.Structs.Graphics.GeometryName Index), (System.Collections.Generic.Dictionary<string, int> Attributes, int Indices)>;

namespace Akizuki.Conversion.Utility;

public record ModelResourceContext(
	bool IsEvent,
	string ModelPath,
	string TexturesPath,
	Dictionary<string, HashSet<string>> HardPoints,
	Dictionary<string, string> PortPoints,
	Dictionary<string, ModelMiscContext> Filters
) {
	public HashSet<string> HandledParts { get; } = [];
}

public record ModelMiscContext(bool IsBlockList, HashSet<string> Filters);

public record ModelBuilderContext(
	IConversionOptions Flags,
	ResourceManager Manager,
	Stream BufferStream,
	ModelResourceContext Resource) {
	public Dictionary<string, int> ThicknessMaterialCache { get; } = [];
	public Dictionary<ResourceId, PrimitiveCache> PrimCache { get; } = [];
	public Dictionary<ResourceId, GeometryCache> GeometryCache { get; } = [];
	public Dictionary<ResourceId, int> MaterialCache { get; } = [];
	public Dictionary<ResourceId, int> TextureCache { get; } = [];
}

public record CamouflageContext(CamouflageColorScheme? ColorScheme, Camouflage? Camouflage, CamouflagePart Part, Dictionary<string, string> Redirect, List<string> Style);
