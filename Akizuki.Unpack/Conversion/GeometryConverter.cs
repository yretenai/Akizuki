// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Text.Json;
using Akizuki.Graphics;
using Akizuki.Structs.Graphics;
using BCDecNet;
using DragonLib.IO;
using Triton;
using Triton.Encoder;
using Triton.Pixel.Formats;

namespace Akizuki.Unpack.Conversion;

internal static class GeometryConverter {
	internal static bool ConvertSplash(string path, ProgramFlags flags, IMemoryBuffer<byte> data) {
		var splash = new Splash(data);

		if (flags.Dry) {
			return true;
		}

		using var stream = new FileStream(path + ".json", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(stream, splash, Program.Options);
		stream.WriteByte((byte) '\n');
		return true;
	}

	public static bool ConvertTexture(string path, ProgramFlags flags, IMemoryBuffer<byte> data) {
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

		var ext = Path.GetExtension(path);
		if (ext != ".dds") {
			path = Path.ChangeExtension(path, $".{ext[^1]}.{imageFormat.ToString().ToLowerInvariant()}");
		} else {
			path = Path.ChangeExtension(path, $".{imageFormat.ToString().ToLowerInvariant()}");
		}

		using var texture = new DDSTexture(data);
		if (texture.OneMipSize == 0 ) {
			return false;
		}

		using var collection = new ImageCollection();

		var width = texture.Width;
		var height = texture.Height;

		var numSurfaces = imageFormat == TextureFormat.TIF ? texture.Surfaces : 1;
		for (var index = 0; index < numSurfaces; ++index) {
			var chunk = texture.GetSurface(index);
			var chunkMem = chunk.Memory;
			if (texture.Format is >= DXGIFormat.BC1_TYPELESS and <= DXGIFormat.BC5_SNORM or >= DXGIFormat.BC6H_TYPELESS and <= DXGIFormat.BC7_UNORM_SRGB) {
				var frameBuffer = new MemoryBuffer<byte>(width * height * 16);
				try {
					var frameBufferMem = frameBuffer.Memory;
					switch (texture.Format) {
						case DXGIFormat.BC1_UNORM:
						case DXGIFormat.BC1_UNORM_SRGB:
							BCDec.DecompressBC1(chunkMem, frameBufferMem, width, height);
							collection.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC2_UNORM:
						case DXGIFormat.BC2_UNORM_SRGB:
							BCDec.DecompressBC2(chunkMem, frameBufferMem, width, height);
							collection.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC3_UNORM:
						case DXGIFormat.BC3_UNORM_SRGB:
							BCDec.DecompressBC3(chunkMem, frameBufferMem, width, height);
							collection.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC4_UNORM:
						case DXGIFormat.BC4_SNORM:
							BCDec.DecompressBC4(chunkMem, frameBufferMem, width, height, texture.Format == DXGIFormat.BC4_SNORM);
							collection.Add(new ImageBuffer<ColorR<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC5_SNORM:
							BCDec.DecompressBC5(chunkMem, frameBufferMem, width, height, true);
							collection.Add(new ImageBuffer<ColorRG<sbyte>, sbyte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC5_UNORM:
							BCDec.DecompressBC5(chunkMem, frameBufferMem, width, height, false);
							collection.Add(new ImageBuffer<ColorRG<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC6H_SF16:
						case DXGIFormat.BC6H_UF16: {
							var isSigned = texture.Format == DXGIFormat.BC6H_SF16;
							BCDec.DecompressBC6HFloat(chunkMem, frameBufferMem, width, height, isSigned);
							collection.Add(new ImageBuffer<ColorRGB<float>, float>(frameBuffer, width, height, isSigned));
							continue;
						}
						case DXGIFormat.BC7_UNORM:
						case DXGIFormat.BC7_UNORM_SRGB:
							BCDec.DecompressBC7(chunkMem, frameBufferMem, width, height);
							collection.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
					}
				} catch {
					frameBuffer.Dispose();
					throw;
				}

				frameBuffer.Dispose();
				throw new UnreachableException();
			}

			switch (texture.Format) {
				case DXGIFormat.A8_UNORM:
				case DXGIFormat.R8_UNORM:
				case DXGIFormat.R8_SNORM:
					collection.Add(new ImageBuffer<ColorR<byte>, byte>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8_SINT:
				case DXGIFormat.R8G8_SNORM:
					collection.Add(new ImageBuffer<ColorRG<sbyte>, sbyte>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8_UNORM:
				case DXGIFormat.R8G8_UINT:
					collection.Add(new ImageBuffer<ColorRG<byte>, byte>(chunk, width, height));
					continue;
				case DXGIFormat.R16_SINT:
				case DXGIFormat.R16_SNORM:
					collection.Add(new ImageBuffer<ColorR<short>, short>(chunk, width, height));
					continue;
				case DXGIFormat.R16_UINT:
				case DXGIFormat.R16_UNORM:
					collection.Add(new ImageBuffer<ColorR<ushort>, ushort>(chunk, width, height));
					continue;
				case DXGIFormat.R16_FLOAT:
					collection.Add(new ImageBuffer<ColorR<Half>, Half>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8B8A8_UNORM:
				case DXGIFormat.R8G8B8A8_UNORM_SRGB:
				case DXGIFormat.R8G8B8A8_UINT:
					collection.Add(new ImageBuffer<ColorRGBA<byte>, byte>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8B8A8_SNORM:
				case DXGIFormat.R8G8B8A8_SINT:
					collection.Add(new ImageBuffer<ColorRGBA<sbyte>, sbyte>(chunk, width, height));
					continue;
				case DXGIFormat.R32_FLOAT:
					collection.Add(new ImageBuffer<ColorR<float>, float>(chunk, width, height));
					continue;
				case DXGIFormat.R32_SINT:
					collection.Add(new ImageBuffer<ColorR<int>, int>(chunk, width, height));
					continue;
				case DXGIFormat.R32_UINT:
					collection.Add(new ImageBuffer<ColorR<uint>, uint>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16_FLOAT:
					collection.Add(new ImageBuffer<ColorRG<Half>, Half>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16_UNORM:
				case DXGIFormat.R16G16_UINT:
					collection.Add(new ImageBuffer<ColorRG<ushort>, ushort>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16_SNORM:
				case DXGIFormat.R16G16_SINT:
					collection.Add(new ImageBuffer<ColorRG<short>, short>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16B16A16_FLOAT:
					collection.Add(new ImageBuffer<ColorRGBA<Half>, Half>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16B16A16_SINT:
				case DXGIFormat.R16G16B16A16_SNORM:
					collection.Add(new ImageBuffer<ColorRGBA<short>, short>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16B16A16_UINT:
				case DXGIFormat.R16G16B16A16_UNORM:
					collection.Add(new ImageBuffer<ColorRGBA<ushort>, ushort>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32_FLOAT:
					collection.Add(new ImageBuffer<ColorRG<float>, float>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32_SINT:
					collection.Add(new ImageBuffer<ColorRG<int>, int>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32_UINT:
					collection.Add(new ImageBuffer<ColorRG<uint>, uint>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32B32A32_FLOAT:
					collection.Add(new ImageBuffer<ColorRGBA<float>, float>(chunk, width, height));
					continue;
				default:
					return false;
			}
		}

		if (flags.Dry) {
			return true;
		}

		using var stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
		encoder.Write(stream, EncoderWriteOptions.Default, collection);
		GC.KeepAlive(collection);
		GC.KeepAlive(stream);
		return true;
	}
}
