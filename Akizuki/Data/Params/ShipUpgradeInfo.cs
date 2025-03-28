// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace Akizuki.Data.Params;

public class ShipUpgradeInfo {
	public ShipUpgradeInfo() { }

	public ShipUpgradeInfo(GameDataObject data) {
		foreach (var (key, value) in data) {
			if (key is not string keyStr) {
				continue;
			}

			if (value is not GameDataObject gobj) {
				continue;
			}

			if (!gobj.ContainsKey("components")) {
				continue;
			}

			Upgrades[keyStr] = new ShipUpgrade(gobj, keyStr);
		}
	}

	public Dictionary<string, ShipUpgrade> Upgrades { get; set; } = [];
}
