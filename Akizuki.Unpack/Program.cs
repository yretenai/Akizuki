// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using System.Text.Json.Serialization;
using Akizuki.Data;
using Akizuki.Json;
using Akizuki.Json.Silk;
using Akizuki.Unpack.Conversion;
using Akizuki.Unpack.Conversion.Space;
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
			new JsonMatrix4X4ConverterFactory(),
			new JsonVector2DConverterFactory(),
			new JsonVector3DConverterFactory(),
			new JsonVector4DConverterFactory(),
		},
	};

	private static void Main() {
		var flags = CommandLineFlagsParser.ParseFlags<ProgramFlags>();

		AkizukiLog.Logger = new LoggerConfiguration()
							.MinimumLevel.Is(flags.Verbose ? LogEventLevel.Verbose : flags.Quiet ? LogEventLevel.Fatal : flags.LogLevel)
							.WriteTo.Console(theme: AnsiConsoleTheme.Literate)
							.CreateLogger();

		foreach (var idxFile in new FileEnumerator(flags.IndexFiles, "*.idx")) {
			AkizukiLog.Information("Opening {Index}", Path.GetFileNameWithoutExtension(idxFile));
			using var pfs = new PackageFileSystem(flags.PackageDirectory, idxFile, flags.Validate);

			if (!flags.Dry) {
				var path = Path.Combine(flags.OutputDirectory, "idx", Path.GetFileName(idxFile));
				var dir = Path.GetDirectoryName(path) ?? flags.OutputDirectory;
				Directory.CreateDirectory(dir);
				AssetPaths.Save(path, flags, pfs);
			}

			foreach (var file in pfs.Files) {
				var path = Path.Combine(flags.OutputDirectory, pfs.Paths.TryGetValue(file.Id, out var name) ? name.TrimStart('/', '.') : $"res/unknown/{file.Id:x16}.bin");
			#if DEBUG
				if (!path.EndsWith("assets.bin")) {
					continue;
				}
			#endif

				AkizukiLog.Information("{Value}", name ?? $"{file.Id:x16}");

				using var data = pfs.OpenFile(file);
				if (data == null) {
					AkizukiLog.Warning("Failed ot open {File}", name ?? $"{file.Id:x16}");
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
		}
	}

	private static bool Convert(string path, ProgramFlags flags, IMemoryBuffer<byte> data) {
		var ext = Path.GetExtension(path).ToLowerInvariant();
		var name = Path.GetFileName(path).ToLowerInvariant();

		switch (ext) {
			case ".dd2": // 8k
			case ".dd1": // 4k
			case ".dd0": // 2k
			case ".dds": // 1k
				// todo
				break;
			case ".geometry":
				// todo
				break;
			case ".splash":
				// todo
				break;
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
						return TerrainConverter.Convert(path, flags, data);
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
						var assets = new BigWorldDatabase(data);
						AssetPaths.Save(path, flags, assets);
						Assets.Save(flags, assets);
						return false; // always save assets.bin
					}
				}

				break;
		}

		return false;
	}
}
