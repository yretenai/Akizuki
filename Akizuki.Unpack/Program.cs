// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using System.Text.Json.Serialization;
using Akizuki.Json;
using Akizuki.Json.Silk;
using Akizuki.Unpack.Conversion;
using DragonLib.CommandLine;
using DragonLib.IO;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Akizuki.Unpack;

internal static class Program {
	internal static JsonSerializerOptions Options { get; } = new() {
		WriteIndented = true,
		NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
		NewLine = "\n",
		Converters = {
			new JsonStringEnumConverter(),
			new JsonBigWorldDatabaseConverter(),
			new JsonPackageFileSystemConverter(),
			new JsonStringIdConverter(),
			new JsonResourceIdConverter(),
			new JsonMatrix4X4ConverterFactory(),
			new JsonVector2DConverterFactory(),
			new JsonVector3DConverterFactory(),
			new JsonVector4DConverterFactory(),
		},
	};

	internal static JsonSerializerOptions SafeOptions { get; } = new() {
		WriteIndented = true,
		NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
		NewLine = "\n",
		Converters = {
			new JsonFauxDictionaryConverter(),
			new JsonMatrix4X4ConverterFactory(),
			new JsonVector2DConverterFactory(),
			new JsonVector3DConverterFactory(),
			new JsonVector4DConverterFactory(),
		},
	};

	private static HashSet<string> Names { get; } = [];

	private static void Main() {
		var flags = CommandLineFlagsParser.ParseFlags<ProgramFlags>();

		AkizukiLog.Logger = new LoggerConfiguration()
							.MinimumLevel.Is(flags.Verbose ? LogEventLevel.Verbose : flags.Quiet ? LogEventLevel.Fatal : flags.LogLevel)
							.WriteTo.Console(theme: AnsiConsoleTheme.Literate)
							.CreateLogger();

		using var manager = new ResourceManager(flags.IndexDirectory, flags.PackageDirectory, flags.Validate);

		if (flags.Convert) {
			if (manager.Database != null) {
				var path = Path.Combine(flags.OutputDirectory, "res/content/assets.bin");
				var dir = Path.GetDirectoryName(path) ?? flags.OutputDirectory;
				if (!flags.Dry) {
					Directory.CreateDirectory(dir);
				}

				AssetPaths.Save(path, flags, manager.Database);
				Assets.Save(flags, manager.Database);
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

			if ((flags.ShouldConvertAtAll && Convert(path, flags, data)) || flags.Dry) {
				continue;
			}

			using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			stream.Write(data.Span);
		}
	}

	private static bool Convert(string path, ProgramFlags flags, IMemoryBuffer<byte> data) {
		var ext = Path.GetExtension(path).ToLowerInvariant();
		var name = Path.GetFileName(path).ToLowerInvariant();

		if (flags.ConvertLoose && ext == ".geometry") {
			GeometryConverter.ConvertLooseGeometry(path, flags, data);
			return true;
		}

		if (!flags.Convert) {
			return false;
		}

		switch (ext) {
			case ".dd2": // 2x
			case ".dd1": // 4x
			case ".dd0": // 8x
			case ".dds": // 1x
				return GeometryConverter.ConvertTexture(path, flags, data);
			case ".splash":
				return GeometryConverter.ConvertSplash(path, flags, data);
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
			case ".data" when flags.AllowGameData: {
				return name switch {
					"gameparams.data" => Assets.SaveData(path, flags, data),
					"uiparams.data" => false, // todo
					_ => false,
				};
			}
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
