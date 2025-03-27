// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Conversion;
using DragonLib.CommandLine;
using DragonLib.IO;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Akizuki.Unpack;

internal static class Program {
	internal static ProgramFlags Flags { get; private set; } = null!;

	private static void Main() {
		var flags = Flags = CommandLineFlagsParser.ParseFlags<ProgramFlags>();

		AkizukiLog.Logger = new LoggerConfiguration()
							.MinimumLevel.Is(flags.Verbose ? LogEventLevel.Verbose : flags.Quiet ? LogEventLevel.Fatal : flags.LogLevel)
							.WriteTo.Console(theme: AnsiConsoleTheme.Literate)
							.CreateLogger();

		using var manager = new ResourceManager(flags.IndexDirectory, flags.PackageDirectory, flags.Validate);

		if (manager.GameParams != null && flags.Convert) {
			var path = Path.Combine(flags.OutputDirectory, "res/content/GameParams.data");
			var dir = Path.GetDirectoryName(path) ?? flags.OutputDirectory;
			if (!flags.Dry) {
				Directory.CreateDirectory(dir);
			}

			Assets.SaveData(path, flags, ShouldProcess, manager.GameParams);
		}

		if (flags.Convert) {
			if (manager.Database != null) {
				var path = Path.Combine(flags.OutputDirectory, "res/content/assets.bin");
				var dir = Path.GetDirectoryName(path) ?? flags.OutputDirectory;
				if (!flags.Dry) {
					Directory.CreateDirectory(dir);
				}

				if (ShouldProcess("res/content/assets.json")) {
					AkizukiLog.Information("{Path}", "res/content/assets.json");
					AssetPaths.Save(path, flags, manager.Database);
				}

				Assets.Save(flags.OutputDirectory, flags, ShouldProcess, manager.Database);
			}

			foreach (var pfs in manager.Packages) {
				var path = Path.Combine(flags.OutputDirectory, "idx", Path.GetFileName(pfs.Name));
				var dir = Path.GetDirectoryName(path) ?? flags.OutputDirectory;
				if (!flags.Dry) {
					Directory.CreateDirectory(dir);
				}

				AkizukiLog.Information("{Value}", $"idx/{pfs.Name}.json");
				AssetPaths.Save(path, flags, pfs);
			}
		}

		foreach (var fileId in manager.Files) {
			var path = Path.Combine(flags.OutputDirectory, manager.ReversePathLookup.TryGetValue(fileId, out var name) ? name.TrimStart('/', '.') : $"res/unknown/{fileId:x16}.bin");
			if (!ShouldProcess(path)) {
				continue;
			}

			AkizukiLog.Information("{Value}", name ?? $"{fileId:x16}");

			using var data = manager.OpenFile(fileId);
			if (data == null) {
				AkizukiLog.Warning("Failed to open {File}", name ?? $"{fileId:x16}");
				continue;
			}

			var dir = Path.GetDirectoryName(path) ?? flags.OutputDirectory;
			if (!flags.Dry) {
				Directory.CreateDirectory(dir);
			}

			if ((flags.Convert && Convert(path, flags, data)) || flags.Dry) {
				continue;
			}

			using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			stream.Write(data.Span);
		}

		AkizukiLog.Information("Done");
	}

	internal static bool ShouldProcess(string? path) {
		var hasAnyFilters = false;
		if (Flags.Regexes.Count > 0) {
			hasAnyFilters = true;
			if (path != null && Flags.Regexes.Any(regex => regex.IsMatch(path))) {
				return true;
			}
		}

		if (Flags.Filters.Count > 0) {
			hasAnyFilters = true;
			if (path != null && Flags.Filters.Any(filter => path.Contains(filter, StringComparison.OrdinalIgnoreCase))) {
				return true;
			}
		}

		return !hasAnyFilters;
	}

	private static bool Convert(string path, ProgramFlags flags, IMemoryBuffer<byte> data) {
		var ext = Path.GetExtension(path).ToLowerInvariant();
		var name = Path.GetFileName(path).ToLowerInvariant();

		switch (ext) {
			case ".dd2": // 2x
			case ".dd1": // 4x
			case ".dd0": // 8x
			case ".dds": // 1x
				return GeometryConverter.ConvertTexture(path, flags, data);
			case ".splash":
				return GeometryConverter.ConvertSplash(path, flags, data);
			case ".geometry":
				return GeometryConverter.ConvertLooseGeometry(path, flags, data);
			case ".prefab":
				// todo
				break;
			case ".anim":
				// todo
				break;
			case ".wem":
				// todo
				break;
			case ".bnk":
				// todo
				break;
			case ".bin":
				switch (name) {
					case "terrain.bin":
						return SpaceConverter.ConvertTerrain(path, flags, data);
					case "decor.bin":
						// todo
						break;
					case "models.bin":
						// todo
						break;
					case "forest.bin":
						// todo
						break;
					case "space.bin":
						// todo
						break;
					case "assets.bin": {
						return false; // handled separately
					}
				}

				break;
		}

		return false;
	}
}
