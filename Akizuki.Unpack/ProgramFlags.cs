// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using DragonLib.CommandLine;
using Serilog.Events;
using Triton.Encoder;

namespace Akizuki.Unpack;

public enum TextureFormat {
	None,
	Auto,
	PNG,
	TIF,
}

internal record ProgramFlags : CommandLineFlags {
	[Flag("output-directory", Positional = 0, IsRequired = true, Category = "Akizuki")]
	public string OutputDirectory { get; set; } = null!;

	[Flag("package-directory", Positional = 1, IsRequired = true, Category = "Akizuki")]
	public string PackageDirectory { get; set; } = null!;

	[Flag("package-index", Positional = 2, IsRequired = true, Category = "Akizuki")]
	public string IndexDirectory { get; set; } = null!;

	[Flag("log-level", Help = "Log level to output at", Category = "Akizuki")]
	public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

	[Flag("texture-format", Help = "Format to save textures as", Category = "Akizuki")]
	public TextureFormat Format { get; set; } = TextureFormat.Auto;

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

	[Flag("convert-game-params", Help = "Convert GameParams.data", Category = "Akizuki")]
	public bool ConvertGameData { get; set; }

	[Flag("convert-geometry", Help = "Convert loose geometry (warning, this will overwrite proper processing)", Category = "Akizuki")]
	public bool ConvertLooseGeometry { get; set; }

	internal bool ShouldConvertAtAll => Convert || ConvertLooseGeometry;

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
