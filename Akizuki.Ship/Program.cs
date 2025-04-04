// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Akizuki.Conversion;
using Akizuki.Conversion.Utility;
using Akizuki.Data.Params;
using Akizuki.Structs.Data.Camouflage;
using DragonLib.CommandLine;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Akizuki.Ship;

internal static class Program {
	internal static ProgramFlags Flags { get; private set; } = null!;

	private static void Main() {
		var options = CommandLineOptions.Default with {
			HelpDelegate = (flags, instance, options, invoked) => {
				CommandLineOptions.Default.HelpDelegate(flags, instance, options, invoked);
				Console.WriteLine("Ships follow the param identifier format (PXXX####).");
				Console.WriteLine("Can specify modules by adding + followed by the module name.");
				Console.WriteLine("For example: PJSD108_Akizuki+PJUH705_D8_HULL_TOP_2+PJUS804_D8_SUO_TOP_2");
				Console.WriteLine("Not providing any ship will list all ships present.");
				Console.WriteLine("Providing \"list\" as a module will list all modules present.");
			},
		};
		var flags = Flags = CommandLineFlagsParser.ParseFlags<ProgramFlags>(options);

		AkizukiLog.Logger = new LoggerConfiguration()
							.MinimumLevel.Is(flags.Verbose ? LogEventLevel.Verbose : flags.Quiet ? LogEventLevel.Fatal : flags.LogLevel)
							.WriteTo.Console(theme: AnsiConsoleTheme.Literate)
							.CreateLogger();

		using var manager = new ResourceManager(flags.InstallDirectory, flags.Validate);
		if (!manager.Texts.TryGetValue(flags.Language, out var text)) {
			text = manager.Texts.GetValueOrDefault("en", []);
			AkizukiLog.Warning("Could not load language {Language}", flags.Language);
		}

		if (manager.GameParams.Count == 0 || manager.Database == null) {
			return;
		}

		if (flags.ShipSetups.Count == 0 || flags.ShipSetups.Contains("list")) {
			AkizukiLog.Information("Available ships:");

			foreach (var (key, value) in manager.GameParams.OrderBy(x => x.Key)) {
				if (ParamTypeInfo.GetTypeName(value) != "Ship") {
					continue;
				}

				var avail = new ShipParam(manager.GameParams, value);
				AkizukiLog.Information("\t{Name} ({TranslatedName})", key, text.GetTranslation(avail.Index + "_FULL", avail.Index));
			}

			return;
		}

		if (flags.Wildcard) {
			var newSetups = new HashSet<string>();
			var regexes = flags.ShipSetups.Select(x => new Regex(x)).ToList();
			foreach (var (key, value) in manager.GameParams.OrderBy(x => x.Key)) {
				if (ParamTypeInfo.GetTypeName(value) != "Ship") {
					continue;
				}

				if (regexes.Any(regex => regex.IsMatch(key))) {
					newSetups.Add(key);
				}
			}

			flags.ShipSetups = newSetups;
		}

		foreach (var shipSetup in flags.ShipSetups) {
			if (shipSetup.Length == 0) {
				continue;
			}

			var splitParts = shipSetup.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var shipName = splitParts[0];
			var shipParts = splitParts.Length > 1 ? splitParts[1..].ToHashSet() : [];
			if (!manager.GameParams.TryGetValue(shipName, out var shipData)) {
				AkizukiLog.Error("Could not find ship {Name}", shipName);
				continue;
			}

			if (ParamTypeInfo.GetTypeName(shipData) != "Ship") {
				AkizukiLog.Error("{Name} is not a ship type", shipName);
				continue;
			}

			var ship = new ShipParam(manager.GameParams, shipData);

			if (shipParts.Count == 1 && shipParts.Contains("list")) {
				AkizukiLog.Information("Available parts for ship {Name}:", shipName);
				foreach (var (upgradeName, upgrade) in ship.ShipUpgradeInfo.Upgrades) {
					AkizukiLog.Information("\t{Name} ({Type}, {TranslatedName})", upgradeName, upgrade.UpgradeType, text.GetTranslation(upgradeName));
				}

				continue;
			}

			var selectedParts = new List<ShipUpgrade>();
			if (shipParts.Count == 0) {
				// assuming A_Name, B_Name, this will sort it to most upgrades.
				// this is only necessary for the hull.
				var sorted = ship.ShipUpgradeInfo.Upgrades.OrderBy(x => x.Key);
				if (flags.AllModules) {
					selectedParts.AddRange(sorted.Select(x => x.Value));
				} else {
					var grouped = sorted.GroupBy(x => x.Value.UpgradeType);
					selectedParts.AddRange(grouped.Select(group => group.Last().Value));
				}
			} else {
				selectedParts.AddRange(ship.ShipUpgradeInfo.Upgrades
										   .Where(x => shipParts.Contains(x.Key))
										   .Select(x => x.Value).DistinctBy(x => x));
			}

			AkizukiLog.Information("Selected parts for {Name}: {Parts}", shipName, string.Join(", ", selectedParts.Select(x => x.Name)));
			var selectedPartNames = selectedParts.SelectMany(x => x.Components.Values).SelectMany(x => x).ToList();

			var hullModel = string.Empty;
			var hardPoints = new Dictionary<string, HashSet<string>>();
			var planes = new Dictionary<string, string>();
			foreach (var selectedComponent in selectedPartNames) {
				if (ship.ModelPaths.TryGetValue(selectedComponent, out var componentModel)) {
					hullModel = componentModel;
				}

				if (ship.HardpointModelPaths.TryGetValue(selectedComponent, out var componentHardpoints)) {
					foreach (var (key, value) in componentHardpoints) {
						if (!hardPoints.TryGetValue(key, out var hardpoints)) {
							hardpoints = hardPoints[key] = [];
						}

						hardpoints.Add(value);
					}
				}

				if (ship.PlaneModelPaths.TryGetValue(selectedComponent, out var componentPlanes)) {
					foreach (var (key, value) in componentPlanes) {
						planes[key] = value;
					}
				}
			}

			if (string.IsNullOrEmpty(hullModel)) {
				AkizukiLog.Error("Ship {Mame} does not have a hull model?", shipName);
				continue;
			}

			AkizukiLog.Debug("{Hardpoint}: {ModelPath}", "HP_Full", hullModel);
			foreach (var (key, value) in hardPoints) {
				AkizukiLog.Debug("{Hardpoint}: {ModelPath}", key, value);
			}

			foreach (var (key, value) in planes) {
				AkizukiLog.Debug("{Hardpoint}: {ModelPath}", key, value);
			}

			CamouflageContext? camouflageContext = default;
			var permoflage = ship.NativePermoflage;
			if (string.IsNullOrEmpty(permoflage) && flags.UsePermoflageRegardless) {
				permoflage = ship.Permoflages.FirstOrDefault();
			}

			if (TryFindPermoflageTag(manager.GameParams, permoflage, out var permoflageTag)) {
				var camouflage = manager.Camouflages?.Camouflages.FirstOrDefault(x => x.IsValidFor(permoflageTag, shipName));
				if (camouflage is not null) {
					var colorSchemeId = camouflage.ColorSchemes?.ElementAtOrDefault(ship.CamouflageColorSchemeId) ?? camouflage.ColorSchemes?.FirstOrDefault();
					var colorScheme = colorSchemeId is null ? default : manager.Camouflages!.ColorSchemes.FirstOrDefault(x => x.Name == colorSchemeId);
					var redirect = new Dictionary<string, string>();
					var filter = new HashSet<string>();
					ProcessPermoflagePeculiarities(manager.GameParams, permoflage, selectedPartNames, redirect, hardPoints, filter, ref hullModel);
					camouflageContext = new CamouflageContext(colorScheme, camouflage, CamouflagePart.Unknown, redirect, filter);
				}
			}

			GeometryConverter.ConvertVisual(manager, shipName, flags.OutputDirectory, hullModel, hardPoints, flags, ship.ParamTypeInfo, camouflageContext);

			camouflageContext = camouflageContext != default ? camouflageContext with { Part = CamouflagePart.Plane } : default;

			foreach (var (planeName, planeModel) in planes) {
				GeometryConverter.ConvertVisual(manager, planeName, flags.OutputDirectory, planeModel, hardPoints, flags, ship.ParamTypeInfo, camouflageContext, "plane", shipName);
			}
		}
	}

	private static bool TryFindPermoflageTag(Dictionary<string, Dictionary<object, object>> pickle, [NotNullWhen(true)] string? permoflage, [MaybeNullWhen(false)] out string tag) {
		if (string.IsNullOrEmpty(permoflage)) {
			tag = null;
			return false;
		}

		if (pickle.TryGetValue(permoflage, out var permoflageParams)) {
			if (permoflageParams.GetValueOrDefault<string>("camouflage") is { Length: > 0 } camouflage) {
				tag = camouflage;
				return true;
			}
		}

		tag = null;
		return false;
	}

	private static void ProcessPermoflagePeculiarities(Dictionary<string, Dictionary<object, object>> pickle, string permoflage, List<string> parts, Dictionary<string, string> redirect, Dictionary<string, HashSet<string>> hardpoints, HashSet<string> filter, ref string hullModel) {
		if (!pickle.TryGetValue(permoflage, out var permoflageParams)) {
			return;
		}

		var values = permoflageParams.GetValueOrDefault<Dictionary<object, object>>("peculiarityModels", []);
		foreach (var (key, value) in values) {
			redirect[(string) key] = (string) value;
		}

		values = permoflageParams.GetValueOrDefault<Dictionary<object, object>>("hullConfig", []);
		foreach (var part in parts) {
			if (values.GetValueOrDefault<Dictionary<object, object>>(part) is { } hullConfig) {
				hullModel = hullConfig.GetValueOrDefault("model", hullModel);
			}
		}

		values = permoflageParams.GetValueOrDefault<Dictionary<object, object>>("nodesConfig", []);
		foreach (var part in parts) {
			if (values.GetValueOrDefault<Dictionary<object, object>>(part) is not { } nodeConfig) {
				continue;
			}

			foreach (var (key, value) in nodeConfig) {
				if (value is not Dictionary<object, object> nodeData) {
					continue;
				}

				if (key is not string hardpoint) {
					continue;
				}

				if (nodeData.GetValueOrDefault<string>("model") is not { } model) {
					continue;
				}

				if (!hardpoints.TryGetValue(hardpoint, out var hardpointSet)) {
					hardpointSet = hardpoints[hardpoint] = [];
				}

				hardpointSet.Clear();
				hardpointSet.Add(model);

				if (nodeData.GetValueOrDefault<object[]>("miscFilter") is not { } miscFilter) {
					continue;
				}

				foreach (var misc in miscFilter.OfType<string>()) {
					filter.Add(misc);
				}
			}
		}
	}
}
