// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Space;
using DragonLib.IO;
using Triton;
using Triton.Encoder;
using Triton.Pixel.Formats;

namespace Akizuki.Unpack.Conversion.Space;

public static class TerrainConverter {
	public static bool Convert(string path, IMemoryBuffer<byte> data) {
		IEncoder encoder;
		if (TIFFEncoder.IsAvailable) {
			path = Path.ChangeExtension(path, ".tif");
			encoder = new TIFFEncoder(TIFFCompression.None, TIFFCompression.None);
		} else if(PNGEncoder.IsAvailable) {
			path = Path.ChangeExtension(path, ".png");
			encoder = new PNGEncoder(PNGCompressionLevel.SuperFast);
		} else {
			return false;
		}

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
		
		using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		encoder.Write(stream, EncoderWriteOptions.Default, [heightmap]);
		return true;
	}
}
