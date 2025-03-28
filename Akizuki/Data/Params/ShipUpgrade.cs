// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace Akizuki.Data.Params;

public class ShipUpgrade {
	public ShipUpgrade() { }

	public ShipUpgrade(GameDataObject data, string name) {
		Name = name;
		UpgradeType = Enum.Parse<ShipUpgradeType>(data.GetValueOrDefault("ucType", "_None").TrimStart('_'));
		Prev = data.GetValueOrDefault("prev", string.Empty);

		var components = data.GetValueOrDefault<GameDataObject>("components", []);
		foreach (var (componentName, componentData) in components) {
			if (componentName is not string nameStr) {
				continue;
			}

			if (componentData is not IEnumerable<object> values) {
				continue;
			}

			if (!Components.TryGetValue(nameStr, out var componentList)) {
				componentList = Components[nameStr] = [];
			}

			componentList.AddRange(values.OfType<string>());
		}
	}

	public string Name { get; set; } = "";
	public Dictionary<string, List<string>> Components { get; set; } = [];
	public string Prev { get; set; } = string.Empty;
	public ShipUpgradeType UpgradeType { get; set; }
}
