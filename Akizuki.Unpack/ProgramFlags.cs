// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using DragonLib.CommandLine;
using Serilog.Events;

namespace Akizuki.Unpack;

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

	[Flag("dry", Help = "Only load (and verify) packages, don't write data", Category = "Akizuki")]
	public bool Dry { get; set; }

	[Flag("convert", Help = "Convert data to common formats", Category = "Akizuki")]
	public bool Convert { get; set; }

	[Flag("convert-geometry", Help = "Convert loose geometry", Category = "Akizuki")]
	public bool ConvertLoose { get; set; }

	public bool ShouldConvertAtAll => Convert || ConvertLoose;
}
