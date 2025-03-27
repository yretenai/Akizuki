// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using DragonLib.CommandLine;
using Serilog.Events;
using Triton.Encoder;

namespace Akizuki.Unpack;

internal enum TextureFormat {
	None,
	Auto,
	PNG,
	TIF,
}

internal record ProgramFlags : CommandLineFlags {
	[Flag("output-directory", Positional = 0, IsRequired = true, Category = "Akizuki")]
	internal string OutputDirectory { get; set; } = null!;

	[Flag("package-directory", Positional = 1, IsRequired = true, Category = "Akizuki")]
	internal string PackageDirectory { get; set; } = null!;

	[Flag("package-index", Positional = 2, IsRequired = true, Category = "Akizuki")]
	internal string IndexDirectory { get; set; } = null!;

	[Flag("log-level", Help = "Log level to output at", Category = "Akizuki")]
	internal LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

	[Flag("texture-format", Help = "Format to save textures as", Category = "Akizuki")]
	internal TextureFormat Format { get; set; } = TextureFormat.Auto;

#if DEBUG
	[Flag("verbose", Help = "Set log level to the highest possible level", Category = "Akizuki")]
#endif
	internal bool Verbose { get; set; }

	[Flag("quiet", Help = "Set log level to the lowest possible level", Category = "Akizuki")]
	internal bool Quiet { get; set; }

	[Flag("validate", Help = "Verify if package data is corrupt or not", Category = "Akizuki")]
	internal bool Validate { get; set; }

	[Flag("dry", Help = "Only load (and verify) packages, don't write data", Category = "Akizuki")]
	internal bool Dry { get; set; }

	[Flag("convert", Help = "Convert data to common formats", Category = "Akizuki")]
	internal bool Convert { get; set; }

	[Flag("allow-game-params", Help = "Convert GameParams.data (WARNING, the Json file is not well formed and 1+GiB)", Category = "Akizuki")]
	internal bool AllowGameData { get; set; }

	[Flag("convert-geometry", Help = "Convert loose geometry", Category = "Akizuki")]
	internal bool ConvertLoose { get; set; }

	internal bool ShouldConvertAtAll => Convert || ConvertLoose;

	internal TextureFormat SelectedFormat {
		get {
			if (Format != TextureFormat.Auto) {
				return Format;
			}

			if (TIFFEncoder.IsAvailable) {
				Format = TextureFormat.TIF;
			} else if (PNGEncoder.IsAvailable) {
				Format = TextureFormat.PNG;
			} else {
				Format = TextureFormat.None;
			}

			return Format;
		}
	}

	internal IEncoder? FormatEncoder =>
		SelectedFormat switch {
			TextureFormat.PNG when PNGEncoder.IsAvailable => new PNGEncoder(PNGCompressionLevel.SuperSmall),
			TextureFormat.TIF when TIFFEncoder.IsAvailable => new TIFFEncoder(TIFFCompression.Deflate, TIFFCompression.Deflate),
			TextureFormat.None => null,
			TextureFormat.Auto => null,
			_ => null,
		};
}
