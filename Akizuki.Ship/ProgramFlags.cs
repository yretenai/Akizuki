// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Conversion.Utility;
using DragonLib.CommandLine;
using Serilog.Events;
using Triton;
using Triton.Encoder;

namespace Akizuki.Ship;

internal record ProgramFlags : CommandLineFlags, IConversionOptions {
	[Flag("output-directory", Positional = 0, IsRequired = true, Category = "Akizuki")]
	public string OutputDirectory { get; set; } = null!;

	[Flag("install-directory", Positional = 1, IsRequired = true, Category = "Akizuki")]
	public string InstallDirectory { get; set; } = null!;

	[Flag("ship", Help = "List of ships to convert", Positional = 2, Category = "Akizuki")]
	public HashSet<string> ShipSetups { get; set; } = [];

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

	[Flag("wildcard", Help = "Assume ship names are regexes (disables part selection)", Category = "Akizuki")]
	public bool Wildcard { get; set; }

	[Flag("all-modules", Help = "Export all possible modules instead of pinnacle when no modules are provided", Category = "Akizuki")]
	public bool AllModules { get; set; }

	[Flag("language", Help = "Language locale to load", Category = "Akizuki")]
	public string Language { get; set; } = "en";

	[Flag("permoflage", Help = "Use Permoflages on ships that do not have a native permoflage", Category = "Akizuki")]
	public bool UsePermoflageRegardless { get; set; }

	[Flag("secondary-permoflage", Help = "Convert the default permoflage in addition to base skin (does nothing if a native permoflage is present)", Category = "Akizuki")]
	public bool SecondaryPermoflage { get; set; }

	[Flag("all-permoflages", Help = "Convert all permoflages a ship has", Category = "Akizuki")]
	public bool AllPermoflages { get; set; }

	[Flag("texture-format", Help = "Format to save textures as", Category = "Akizuki")]
	public TextureFormat ImageFormat { get; set; } = TextureFormat.Auto;

	[Flag("type-info", Help = "Insert type information in the resulting file path", Category = "Akizuki")]
	public bool InsertTypeInfo { get; set; }

	[Flag("blender", Help = "Make glTF Materials safe for usage in blender", Category = "Akizuki")]
	public bool BlenderSafe { get; set; }

	[Flag("dry", Help = "Only load (and verify) packages, don't write data", Category = "Akizuki")]
	public bool Dry { get; set; }

	[Flag("convert-armor", Help = "Convert armor models with geometry to glTF", Category = "Akizuki")]
	public bool ConvertArmor { get; set; }

	public CubemapStyle CubemapStyle => CubemapStyle.Equirectangular;
	public bool ConvertTextures => true;
	public bool ConvertCubeMaps => true;

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
