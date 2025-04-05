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
				Console.WriteLine("\tPJSD108_Akizuki+AB1_Artillery+AB1_Torpedoes");
				Console.WriteLine("Can specify a specific permoflage by adding @ followed by the skin identifier after the ship name.");
				Console.WriteLine("\tPJSD108_Akizuki@PCEM002_Halloween19_8lvl+AB1_Artillery+AB1_Torpedoes");
				Console.WriteLine("Providing \"list\" as a module will list all modules and permoflages present.");
				Console.WriteLine("\tPJSD108_Akizuki+list");
				Console.WriteLine("Not providing any ship will list all ships present.");
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
			var atIndex = shipName.IndexOf('@', StringComparison.Ordinal);
			string? selectedPermoflage = null;
			if (atIndex > -1) {
				selectedPermoflage = shipName[(atIndex + 1)..];
				shipName = shipName[..atIndex];
			}

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
					foreach (var partName in upgrade.Components) {
						AkizukiLog.Information("\t\t{PartName}", partName);
					}
				}

				AkizukiLog.Information("Available permoflages for ship {Name}:", shipName);
				foreach (var permoflageName in ship.Permoflages) {
					AkizukiLog.Information("\t{Name} ({TranslatedName})", permoflageName, text.GetTranslation(permoflageName));
				}

				continue;
			}

			HashSet<string> selectedParts;
			if (shipParts.Count == 0) {
				// assuming A_Name, B_Name, this will sort it to most upgrades.
				// this is only necessary for the hull.
				var sorted = ship.ShipUpgradeInfo.Upgrades.OrderBy(x => x.Key);
				IEnumerable<ShipUpgrade> selectedPartSelector;
				if (flags.AllModules) {
					// todo: this breaks misc resolution
					selectedPartSelector = sorted.Select(x => x.Value);
				} else {
					var grouped = sorted.GroupBy(x => x.Value.UpgradeType);
					selectedPartSelector = grouped.Select(group => group.Last().Value);
				}

				selectedParts = new HashSet<string>(selectedPartSelector.SelectMany(x => x.Components.Values).Where(x => x.Count > 0).Select(x => x.Last()));
			} else {
				selectedParts = shipParts;
			}

			AkizukiLog.Information("Selected parts for {Name}: {Parts}", shipName, string.Join(", ", selectedParts));

			var hullModel = string.Empty;
			var hardPoints = new Dictionary<string, HashSet<string>>();
			var planes = new Dictionary<string, string>();
			var filters = new Dictionary<string, ModelMiscContext>();
			foreach (var selectedComponent in selectedParts) {
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

				if (ship.Filters.TryGetValue(selectedComponent, out var componentFilters)) {
					foreach (var (key, values) in componentFilters) {
						if (!filters.TryGetValue(key, out var filter)) {
							filter = filters[key] = new ModelMiscContext(values.IsBlockList, []);
						}

						foreach (var value in values.Filters) {
							filter.Filters.Add(value);
						}
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

			if (string.IsNullOrEmpty(selectedPermoflage) && ship.Permoflages.Count > 0) {
				if (flags.AllPermoflages) {
					ProcessShip(flags, manager, shipName, null, ship, selectedParts, hardPoints, filters, hullModel, planes, shipName);

					foreach (var permoflage in ship.Permoflages) {
						ProcessShip(flags, manager, permoflage, permoflage, ship, selectedParts, hardPoints, filters, hullModel, planes, shipName, true);
					}

					continue;
				}

				if (string.IsNullOrEmpty(ship.NativePermoflage) && (flags.SecondaryPermoflage || flags.UsePermoflageRegardless)) {
					if (flags.SecondaryPermoflage) {
						ProcessShip(flags, manager, shipName, null, ship, selectedParts, hardPoints, filters, hullModel, planes, shipName);
					}

					selectedPermoflage = ship.Permoflages
											 .Select(x => (Key: x, Value: manager.GameParams.GetValueOrDefault(x)))
											 .Where(x => x.Value != null)
											 .OrderByDescending(x => x.Value!.GetValueOrDefault<int>("sortOrder")).First().Key;
				} else {
					selectedPermoflage = ship.NativePermoflage;
				}
			}

			if (string.IsNullOrEmpty(selectedPermoflage)) {
				selectedPermoflage = null;
			}

			ProcessShip(flags, manager, selectedPermoflage ?? shipName, selectedPermoflage, ship, selectedParts, hardPoints, filters, hullModel, planes, shipName);
		}
	}

	private static void ProcessShip(ProgramFlags flags, ResourceManager manager, string modelName, string? permoflage, ShipParam ship,
		HashSet<string> selectedPartNames, Dictionary<string, HashSet<string>> hardPoints, Dictionary<string, ModelMiscContext> filters,
		string hullModel, Dictionary<string, string> planes,
		string shipName, bool enforcePermoflage = false) {
		CamouflageColorScheme? colorScheme = default;
		Camouflage? camouflage = default;
		if (TryFindPermoflageTag(manager.GameParams, permoflage, out var permoflageTag)) {
			camouflage = manager.Camouflages?.Camouflages.FirstOrDefault(x => x.IsValidFor(permoflageTag, shipName));
			if (camouflage is not null) {
				var colorSchemeId = camouflage.ColorSchemes?.ElementAtOrDefault(ship.CamouflageColorSchemeId) ?? camouflage.ColorSchemes?.FirstOrDefault();
				colorScheme = colorSchemeId is null ? default : manager.Camouflages!.ColorSchemes.FirstOrDefault(x => x.Name == colorSchemeId);
			}
		}

		var camouflageContext = ProcessPermoflagePeculiarities(manager.GameParams, permoflage, colorScheme, camouflage, selectedPartNames, hardPoints, filters, ref hullModel);

		if (enforcePermoflage && camouflageContext is null) {
			return;
		}

		GeometryConverter.ConvertVisual(manager, modelName, flags.OutputDirectory, hullModel, hardPoints, filters, flags, ship.ParamTypeInfo, camouflageContext, null, shipName);

		camouflageContext = camouflageContext != default ? camouflageContext with { Part = CamouflagePart.Plane } : default;

		foreach (var (planeName, planeModel) in planes) {
			GeometryConverter.ConvertVisual(manager, planeName, flags.OutputDirectory, planeModel, hardPoints, filters, flags, ship.ParamTypeInfo, camouflageContext, $"plane/{modelName}", shipName);
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

	private static CamouflageContext? ProcessPermoflagePeculiarities(
		Dictionary<string, Dictionary<object, object>> pickle, string? permoflage,
		CamouflageColorScheme? colorScheme, Camouflage? camouflage,
		HashSet<string> parts, Dictionary<string, HashSet<string>> hardpoints, Dictionary<string, ModelMiscContext> filters,
		ref string hullModel) {
		if (string.IsNullOrEmpty(permoflage)) {
			return null;
		}

		if (!pickle.TryGetValue(permoflage, out var permoflageParams)) {
			return null;
		}

		var redirect = new Dictionary<string, string>();
		var style = new List<string>();

		var values = permoflageParams.GetValueOrDefault<Dictionary<object, object>>("peculiarityModels", []);
		foreach (var (key, value) in values) {
			redirect[(string) key] = (string) value;
		}

		var subTypes = permoflageParams.GetValueOrDefault<object[]>("subTypes", []);
		style.AddRange(subTypes.OfType<string>());

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

				filters[hardpoint] = new ModelMiscContext(nodeData.GetValueOrDefault<bool>("miscFilterMode"), [..miscFilter.OfType<string>()]);
			}
		}

		return new CamouflageContext(colorScheme, camouflage, CamouflagePart.Unknown, redirect, style);
	}
}
