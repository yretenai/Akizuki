// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using Akizuki.Data.Params;
using DragonLib.CommandLine;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Akizuki.Ship;

internal static class Program {
	internal static ProgramFlags Flags { get; private set; } = null!;

	private static void Main() {
		var flags = Flags = CommandLineFlagsParser.ParseFlags<ProgramFlags>();

		AkizukiLog.Logger = new LoggerConfiguration()
							.MinimumLevel.Is(flags.Verbose ? LogEventLevel.Verbose : flags.Quiet ? LogEventLevel.Fatal : flags.LogLevel)
							.WriteTo.Console(theme: AnsiConsoleTheme.Literate)
							.CreateLogger();

		using var manager = new ResourceManager(flags.InstallDirectory, flags.Validate);

		if (manager.GameParams == null || manager.Database == null) {
			return;
		}

		if (string.IsNullOrEmpty(flags.ShipName)) {
			AkizukiLog.Information("Available ships:");

			foreach (var (key, value) in manager.GameParams.Values.OrderBy(x => x.Key)) {
				if (TypeInfo.GetTypeName(value) != "Ship") {
					continue;
				}

				var avail = new ShipParam(value);
				AkizukiLog.Information("\t{Name} ({TranslatedName})", key, manager.Text.GetTranslation(avail.Index + "_FULL", avail.Index));
			}

			return;
		}

		if (!manager.GameParams.Values.TryGetValue(flags.ShipName, out var shipData)) {
			AkizukiLog.Error("Could not find {Name}", flags.ShipName);
			return;
		}

		if (TypeInfo.GetTypeName(shipData) != "Ship") {
			AkizukiLog.Error("{Name} is not a ship!", flags.ShipName);
			return;
		}

		var ship = new ShipParam(shipData);

		var selectedParts = new Dictionary<ShipUpgradeType, ShipUpgrade>();
		foreach (var (_, upgrade) in ship.ShipUpgradeInfo.Upgrades) {
			if (string.IsNullOrEmpty(upgrade.Prev)) {
				selectedParts[upgrade.UpgradeType] = upgrade;
			}
		}

		if (flags.ShipParts.Count == 1 && flags.ShipParts.First() == "list") {
			AkizukiLog.Information("Available parts:");
			foreach (var (upgradeName, upgrade) in ship.ShipUpgradeInfo.Upgrades) {
				AkizukiLog.Information("\t{Name} ({Type}, {TranslatedName})", upgradeName, upgrade.UpgradeType, manager.Text.GetTranslation(upgradeName));
			}

			return;
		}

		var selectedComponents = ship.ShipUpgradeInfo.Upgrades
									 .Where(x => flags.ShipParts.Contains(x.Key))
									 .Select(x => x.Value).DistinctBy(x => x);
		foreach (var selectedComponent in selectedComponents) {
			selectedParts[selectedComponent.UpgradeType] = selectedComponent;
		}

		AkizukiLog.Information("Selected parts: {Parts}", string.Join(", ", selectedParts.Values.Select(x => x.Name)));

		var hullModel = string.Empty;
		var hardpoints = new Dictionary<string, string>();
		foreach (var selectedComponent in selectedParts.Values.SelectMany(x => x.Components.Values).SelectMany(x => x)) {
			if (ship.ModelPaths.TryGetValue(selectedComponent, out var componentModel)) {
				hullModel = componentModel;
			}

			if (ship.HardpointModelPaths.TryGetValue(selectedComponent, out var componentHardpoints)) {
				foreach (var (key, value) in componentHardpoints) {
					hardpoints[key] = value;
				}
			}
		}

		if (string.IsNullOrEmpty(hullModel)) {
			throw new UnreachableException();
		}

		AkizukiLog.Debug("{Hardpoint}: {ModelPath}", "HP_Full", hullModel);
		foreach (var (key, value) in hardpoints) {
			AkizukiLog.Debug("{Hardpoint}: {ModelPath}", key, value);
		}
	}
}
