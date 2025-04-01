// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Silk.NET.Maths;

namespace Akizuki.Structs.Data.Camouflage;

public record Camouflage {
	[XmlElement("name")]
	public string Name { get; set; } = string.Empty;

	[XmlElement("annotation")]
	public string? Annotation { get; set; }

	[XmlElement("realm")]
	public string? Realm { get; set; }

	[XmlElement("tiled")]
	public bool Tiled { get; set; }

	[XmlIgnore] // XmlSerializer can't handle "True", what a crap serializer.
	public bool UseColorScheme { get; set; }

	[XmlElement("targetShip")]
	public List<string> TargetShips { get; set; } = [];

	[XmlElement("colorSchemes")]
	public List<string> ColorSchemes { get; set; } = [];

	[XmlElement("shipGroups")] [JsonIgnore]
	public string? ShipGroupsRaw { get; set; }

	[XmlIgnore]
	public HashSet<string>? ShipGroups => ShipGroupsRaw?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();

	[XmlIgnore]
	public Dictionary<string, Vector2D<float>> UV { get; set; } = [];

	[XmlIgnore]
	public Dictionary<string, string>? Shaders { get; set; }

	[XmlIgnore]
	public Dictionary<string, CamouflageTexture> Textures { get; set; } = [];
}
