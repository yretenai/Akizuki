// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using Akizuki.Space;
using DragonLib.IO;
using Triton;
using Triton.Encoder;
using Triton.Pixel.Formats;

namespace Akizuki.Unpack.Conversion;

internal static class SpaceConverter {
	internal static bool ConvertTerrain(string path, ProgramFlags flags, IMemoryBuffer<byte> data) {
		var imageFormat = flags.ValidFormat;
		if (imageFormat == TextureFormat.None) {
			return false;
		}

		IEncoder encoder = imageFormat switch {
			TextureFormat.PNG => new PNGEncoder(PNGCompressionLevel.SuperFast),
			TextureFormat.TIF => new TIFFEncoder(TIFFCompression.None, TIFFCompression.None),
			TextureFormat.None => throw new UnreachableException(),
			_ => throw new UnreachableException(),
		};
		path = Path.ChangeExtension(path, $".{imageFormat.ToString().ToLowerInvariant()}");

		using var terrain = new CompiledTerrain(data);
		using var cast = new CastMemoryBuffer<byte, float>(terrain.Data);
		using var heightmap = new ImageBuffer<ColorR<float>, float>(cast, new Point<int>(terrain.Header.Width, terrain.Header.Height));

		var min = terrain.Header.Min;
		var max = terrain.Header.Max;
		var range = max - min;

		// normalize the color data.
		var span = terrain.Data.Span;
		for (var i = 0; i < span.Length; i++) {
			span[i] = (span[i] - min) / range;
		}

		if (flags.Dry) {
			return true;
		}

		using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		encoder.Write(stream, EncoderWriteOptions.Default, [heightmap]);
		return true;
	}
}
