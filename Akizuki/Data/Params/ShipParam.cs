// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;

namespace Akizuki.Data.Params;

public class ShipParam : ParamsObject {
	public ShipParam() { }

	public ShipParam(GameDataObject data) : base(data) {
		Index = data.GetValue<string>("index");
		Name = data.GetValue<string>("name");
		ShipUpgradeInfo = data.GetParamOrDefault("ShipUpgradeInfo", ShipUpgradeInfo);

		foreach (var component in ShipUpgradeInfo.Upgrades
												 .SelectMany(x => x.Value.Components.Values)
												 .SelectMany(x => x).Distinct()) {
			var componentData = data.GetValueOrDefault<GameDataObject>(component, []);
			if (componentData.TryGetValue<string>("model", out var model) && !string.IsNullOrEmpty(model)) {
				ModelPaths[component] = model;
			}

			foreach (var (key, value) in componentData) {
				if (key is not string keyStr) {
					continue;
				}

				if (value is not GameDataObject componentPointData) {
					continue;
				}

				if (keyStr.Length < 3) {
					continue;
				}

				if (keyStr[..3] is not ("HP_" or "MP_" or "SP_")) {
					continue;
				}

				if (componentPointData.TryGetValue<string>("model", out var hpModel) && !string.IsNullOrEmpty(hpModel)) {
					if (!HardpointModelPaths.TryGetValue(component, out var componentHardpoints)) {
						componentHardpoints = HardpointModelPaths[component] = [];
					}

					if (string.IsNullOrEmpty(componentHardpoints.GetValueOrDefault(keyStr))) {
						componentHardpoints[keyStr] = hpModel;
					} else {
						throw new UnreachableException();
					}
				}
			}
		}
	}

	public string Name { get; set; } = "PXXX000";

	public string Index { get; set; } = "PXXX000";
	public ShipUpgradeInfo ShipUpgradeInfo { get; set; } = new();
	public Dictionary<string, string> ModelPaths { get; set; } = [];
	public Dictionary<string, Dictionary<string, string>> HardpointModelPaths { get; set; } = [];
}
