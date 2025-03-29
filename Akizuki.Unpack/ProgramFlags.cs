// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.RegularExpressions;
using Akizuki.Conversion;
using DragonLib.CommandLine;
using Serilog.Events;
using Triton;
using Triton.Encoder;

namespace Akizuki.Unpack;

internal record ProgramFlags : CommandLineFlags, IConversionOptions {
	[Flag("output-directory", Positional = 0, IsRequired = true, Category = "Akizuki")]
	public string OutputDirectory { get; set; } = null!;

	[Flag("install-directory", Positional = 1, IsRequired = true, Category = "Akizuki")]
	public string InstallDirectory { get; set; } = null!;

	[Flag("expr", Help = "Only handle files that match these regexes", Category = "Akizuki", Extra = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
	public HashSet<Regex> Regexes { get; set; } = [];

	[Flag("filter", Help = "Only handle files that match these strings", Category = "Akizuki")]
	public HashSet<string> Filters { get; set; } = [];

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

	[Flag("save-asset-list", Help = "Save the asset list to JSON", Category = "Akizuki")]
	public bool SaveAssetList { get; set; }

	[Flag("convert", Help = "Convert custom formats to common formats", Category = "Akizuki")]
	public bool Convert { get; set; }

	[Flag("convert-assets", Help = "Convert AssetDB entries to JSON", Category = "Akizuki")]
	public bool ConvertAssetDB { get; set; }

	[Flag("convert-params", Help = "Convert GameParams to JSON", Category = "Akizuki")]
	public bool ConvertGameParams { get; set; }

	[Flag("convert-geometry", Help = "Convert geometry to glTF", Category = "Akizuki")]
	public bool ConvertGeometry { get; set; }

	[Flag("convert-textures", Help = "Convert textures to regular images", Category = "Akizuki")]
	public bool ConvertTextures { get; set; }

	[Flag("convert-cubemaps", Help = "Convert cubemap textures to regular images", Category = "Akizuki")]
	public bool ConvertCubeMaps { get; set; }

	[Flag("texture-format", Help = "Format to save textures as", Category = "Akizuki")]
	public TextureFormat ImageFormat { get; set; } = TextureFormat.Auto;

	[Flag("cubemap-style", Help = "Style to save cubemaps as", Category = "Akizuki")]
	public CubemapStyle CubemapStyle { get; set; } = CubemapStyle.Equirectangular;

	[Flag("dry", Help = "Only load (and verify) packages, don't write data", Category = "Akizuki")]
	public bool Dry { get; set; }

	public bool InsertTypeInfo => false;

	public TextureFormat SelectedFormat {
		get {
			if (ImageFormat != TextureFormat.Auto) {
				return ImageFormat;
			}

			if (TIFFEncoder.IsAvailable) {
				ImageFormat = TextureFormat.TIF;
			} else if (PNGEncoder.IsAvailable) {
				ImageFormat = TextureFormat.PNG;
			} else {
				ImageFormat = TextureFormat.None;
			}

			return ImageFormat;
		}
	}

	public IEncoder? FormatEncoder =>
		SelectedFormat switch {
			TextureFormat.PNG when PNGEncoder.IsAvailable => new PNGEncoder(PNGCompressionLevel.SuperSmall),
			TextureFormat.TIF when TIFFEncoder.IsAvailable => new TIFFEncoder(TIFFCompression.Deflate, TIFFCompression.Deflate),
			TextureFormat.None => null,
			TextureFormat.Auto => null,
			_ => null,
		};
}
