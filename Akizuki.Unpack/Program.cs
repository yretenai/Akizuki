// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

﻿using System.Buffers;
using Akizuki.PFS;
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
			using var pfs = new PFSArchive(flags.PackageDirectory, idxFile, flags.Validate);
			
			foreach (var file in pfs.Files) {
				var path = Path.Combine(flags.OutputDirectory, pfs.Paths.TryGetValue(file.Id, out var name) ? name.TrimStart('/', '.') : $"res/{file.Id:x16}.bin");
				AkizukiLog.Information("{Value}", name ?? $"{file.Id:x16}");

				var data = pfs.OpenFile(file);
				if (data == null) return;

				try {
					var dir = Path.GetDirectoryName(path) ?? flags.OutputDirectory;
					Directory.CreateDirectory(dir);
					using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
					stream.Write(data.AsSpan(0, (int) file.UncompressedSize));
				} finally {
					ArrayPool<byte>.Shared.Return(data);
				}
			}
		}
	}
}

internal record ProgramFlags : CommandLineFlags {
	[Flag("output-directory", Positional = 0, IsRequired = true, Category = "Akizuki")]
	public string OutputDirectory { get; set; } = null!;

	[Flag("package-directory", Positional = 1, IsRequired = true, Category = "Akizuki")]
	public string PackageDirectory { get; set; } = null!;

	[Flag("package-index", Positional = 2, IsRequired = true, Category = "Akizuki")]
	public List<string> IndexFiles { get; set; } = [];

	[Flag("log-level", Help = "Log level to output at", Category = "Akizuki")]
	public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

#if DEBUG
	[Flag("verbose", Help = "Set log level to the highest possible level", Category = "Akizuki")]
#endif
	public bool Verbose { get; set; }

	[Flag("quiet", Help = "Set log level to the lowest possible level", Category = "Akizuki")]
	public bool Quiet { get; set; }

	[Flag("validate", Help = "Verify if package data is corrupt or not", Category = "Akizuki")]
	public bool Validate { get; set; }
}
