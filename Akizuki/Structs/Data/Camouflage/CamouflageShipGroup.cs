// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Akizuki.Structs.Data.Camouflage;

public record CamouflageShipGroup {
	[XmlElement("name")]
	public string Name { get; set; } = string.Empty;

	[XmlElement("ships")] [JsonIgnore]
	public string ShipsRaw { get; set; } = string.Empty;

	[XmlIgnore]
	public HashSet<string> Ships => ShipsRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();
}
