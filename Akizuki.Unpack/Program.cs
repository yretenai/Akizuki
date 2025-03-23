// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Data;
using Akizuki.Unpack.Conversion.Space;
using DragonLib.CommandLine;
using DragonLib.IO;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Akizuki.Unpack;

internal static class Program {
	private static void Main() {
		var flags = CommandLineFlagsParser.ParseFlags<ProgramFlags>();

		AkizukiLog.Logger = new LoggerConfiguration()
							.MinimumLevel.Is(flags.Verbose ? LogEventLevel.Verbose : flags.Quiet ? LogEventLevel.Fatal : flags.LogLevel)
							.WriteTo.Console(theme: AnsiConsoleTheme.Literate)
							.CreateLogger();

		foreach (var idxFile in new FileEnumerator(flags.IndexFiles, "*.idx")) {
			AkizukiLog.Information("Opening {Index}", Path.GetFileNameWithoutExtension(idxFile));
			using var pfs = new PackageFileSystem(flags.PackageDirectory, idxFile, flags.Validate);

			foreach (var file in pfs.Files) {
				var path = Path.Combine(flags.OutputDirectory, pfs.Paths.TryGetValue(file.Id, out var name) ? name.TrimStart('/', '.') : $"res/{file.Id:x16}.bin");
				AkizukiLog.Information("{Value}", name ?? $"{file.Id:x16}");

				using var data = pfs.OpenFile(file);
				if (data == null) {
					AkizukiLog.Warning("Failed ot open {File}", name ?? $"{file.Id:x16}");
					continue;
				}

				if (flags.Dry) {
					continue;
				}

				var dir = Path.GetDirectoryName(path) ?? flags.OutputDirectory;
				Directory.CreateDirectory(dir);

				if (flags.Convert && Convert(path, data)) {
					continue;
				}

				using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
				stream.Write(data.Span);
			}
		}
	}

	private static bool Convert(string path, IMemoryBuffer<byte> data) {
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
						return TerrainConverter.Convert(path, data);
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
					case "assets.bin":
						// todo
						break;
				}

				break;
		}

		return false;
	}
}
