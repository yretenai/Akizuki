// SPDX-FileCopyrightText: 2026 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Xml.Linq;

namespace Akizuki.Camouflage;

public record CamouflageShipGroup {
	public CamouflageShipGroup(XElement shipGroup) {
		Name = (shipGroup.Element("name")?.Value ?? shipGroup.Name.LocalName).Trim();
		if (shipGroup.Element("ships") is { } ships) {
			Ships = ships.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
						 .ToHashSet();
		}
	}

	public string Name { get; set; }

	public HashSet<string> Ships { get; set; } = [];
}
