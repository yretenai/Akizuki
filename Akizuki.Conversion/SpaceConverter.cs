// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using Akizuki.Conversion.Utility;
using Akizuki.Space;
using DragonLib.IO;
using Triton;
using Triton.Encoder;
using Triton.Pixel.Formats;

namespace Akizuki.Conversion;

public static class SpaceConverter {
	[MethodImpl(MethodConstants.Optimize)]
	public static bool ConvertTerrain(string path, IConversionOptions flags, IMemoryBuffer<byte> data) {
		var imageFormat = flags.SelectedFormat;
		if (imageFormat == TextureFormat.None) {
			return false;
		}

		var encoder = flags.FormatEncoder;
		if (encoder == null) {
			return false;
		}

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
