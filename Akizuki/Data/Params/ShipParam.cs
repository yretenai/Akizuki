// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;

namespace Akizuki.Data.Params;

public class ShipParam : ParamObject {
	public ShipParam() { }

	public ShipParam(PickleObject pickle, GameDataObject data) : base(data) {
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

			{
				if (componentData.TryGetValue<string>("planeName", out var planeName) && !string.IsNullOrEmpty(planeName)) {
					if (pickle.TryGetValue(planeName, out var planeData) && planeData.GetValueOrDefault<string>("model") is { } planeModel) {
						if (!HardpointModelPaths.TryGetValue(component, out var componentHardpoints)) {
							componentHardpoints = HardpointModelPaths[component] = [];
						}

						componentHardpoints["HP_Plane_Start"] = planeModel;
					}
				}
			}

			if (componentData.TryGetValue<object[]>("planes", out var planes) && planes.Length > 0 && planes[0] is string) {
				var planeModels = PlaneModelPaths[component] = [];
				foreach (var planeName in planes.OfType<string>()) {
					if (planeName.Length > 0 && pickle.TryGetValue(planeName, out var planeData) && planeData.GetValueOrDefault<string>("model") is { } planeModel) {
						planeModels[planeName] = planeModel;
					}
				}
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
	public Dictionary<string, Dictionary<string, string>> PlaneModelPaths { get; set; } = [];
}
