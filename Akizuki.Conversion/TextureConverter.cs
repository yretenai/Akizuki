// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Akizuki.Conversion.Utility;
using Akizuki.Graphics;
using Akizuki.Structs.Data;
using Akizuki.Structs.Graphics;
using BCDecNet;
using DragonLib.IO;
using GLTF.Scaffold;
using Triton;
using Triton.Encoder;
using Triton.Pixel;
using Triton.Pixel.Channels;
using Triton.Pixel.Formats;

namespace Akizuki.Conversion;

public static class TextureConverter {
	public static StringId NormalMapId { get; } = new(0x4858745d);
	public static StringId MetalMapId { get; } = new(0x89babfe7);

	[MethodImpl(MethodConstants.Optimize)]
	public static string? ConvertTexture(string path, IConversionOptions flags, IMemoryBuffer<byte> data, bool skipExisting = false, bool isNormalMap = false, bool isMetalGlossMap = false) {
		var imageFormat = flags.SelectedFormat;
		if (imageFormat == TextureFormat.None) {
			return null;
		}

		var encoder = flags.FormatEncoder;
		if (encoder == null) {
			return null;
		}

		var ext = Path.GetExtension(path);
		path = Path.ChangeExtension(path,
			ext != ".dds" ? $".{ext[^1]}.{imageFormat.ToString().ToLowerInvariant()}" : $".{imageFormat.ToString().ToLowerInvariant()}");

		if (skipExisting && File.Exists(path)) {
			return path;
		}

		using var texture = new DDSTexture(data);
		if (texture.OneMipSize == 0) {
			return null;
		}

		var isCubemap = texture.IsCubeMap;

		if (!flags.ConvertTextures && !isCubemap) {
			return null;
		}

		if (!flags.ConvertCubeMaps && isCubemap) {
			return null;
		}

		using var frames = new ImageCollection();

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
					// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
					switch (texture.Format) {
						case DXGIFormat.BC1_UNORM:
						case DXGIFormat.BC1_UNORM_SRGB:
							BCDec.DecompressBC1(chunkMem, frameBufferMem, width, height);
							frames.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC2_UNORM:
						case DXGIFormat.BC2_UNORM_SRGB:
							BCDec.DecompressBC2(chunkMem, frameBufferMem, width, height);
							frames.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC3_UNORM:
						case DXGIFormat.BC3_UNORM_SRGB:
							BCDec.DecompressBC3(chunkMem, frameBufferMem, width, height);
							frames.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC4_UNORM:
						case DXGIFormat.BC4_SNORM:
							BCDec.DecompressBC4(chunkMem, frameBufferMem, width, height, texture.Format == DXGIFormat.BC4_SNORM);
							frames.Add(new ImageBuffer<ColorR<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC5_SNORM:
							BCDec.DecompressBC5(chunkMem, frameBufferMem, width, height, true);
							frames.Add(new ImageBuffer<ColorRG<sbyte>, sbyte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC5_UNORM:
							BCDec.DecompressBC5(chunkMem, frameBufferMem, width, height, false);
							frames.Add(new ImageBuffer<ColorRG<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC6H_SF16:
						case DXGIFormat.BC6H_UF16: {
							var isSigned = texture.Format == DXGIFormat.BC6H_SF16;
							BCDec.DecompressBC6HFloat(chunkMem, frameBufferMem, width, height, isSigned);
							frames.Add(new ImageBuffer<ColorRGB<float>, float>(frameBuffer, width, height, isSigned));
							continue;
						}
						case DXGIFormat.BC7_UNORM:
						case DXGIFormat.BC7_UNORM_SRGB:
							BCDec.DecompressBC7(chunkMem, frameBufferMem, width, height);
							frames.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
					}
				} catch {
					frameBuffer.Dispose();
					throw;
				}

				frameBuffer.Dispose();
				throw new UnreachableException();
			}

			// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
			switch (texture.Format) {
				case DXGIFormat.A8_UNORM:
				case DXGIFormat.R8_UNORM:
				case DXGIFormat.R8_SNORM:
				case DXGIFormat.P8:
				case DXGIFormat.IA44: // what are these formats 😭
					frames.Add(new ImageBuffer<ColorR<byte>, byte>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8_SINT:
				case DXGIFormat.R8G8_SNORM:
					frames.Add(new ImageBuffer<ColorRG<sbyte>, sbyte>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8_UNORM:
				case DXGIFormat.R8G8_UINT:
				case DXGIFormat.A8P8:
					frames.Add(new ImageBuffer<ColorRG<byte>, byte>(chunk, width, height));
					continue;
				case DXGIFormat.R16_SINT:
				case DXGIFormat.R16_SNORM:
					frames.Add(new ImageBuffer<ColorR<short>, short>(chunk, width, height));
					continue;
				case DXGIFormat.R16_UINT:
				case DXGIFormat.R16_UNORM:
					frames.Add(new ImageBuffer<ColorR<ushort>, ushort>(chunk, width, height));
					continue;
				case DXGIFormat.R16_FLOAT:
					frames.Add(new ImageBuffer<ColorR<Half>, Half>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8B8A8_UNORM:
				case DXGIFormat.R8G8B8A8_UNORM_SRGB:
				case DXGIFormat.R8G8B8A8_UINT:
					frames.Add(new ImageBuffer<ColorRGBA<byte>, byte>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8B8A8_SNORM:
				case DXGIFormat.R8G8B8A8_SINT:
					frames.Add(new ImageBuffer<ColorRGBA<sbyte>, sbyte>(chunk, width, height));
					continue;
				case DXGIFormat.R32_FLOAT:
					frames.Add(new ImageBuffer<ColorR<float>, float>(chunk, width, height));
					continue;
				case DXGIFormat.R32_SINT:
					frames.Add(new ImageBuffer<ColorR<int>, int>(chunk, width, height));
					continue;
				case DXGIFormat.R32_UINT:
					frames.Add(new ImageBuffer<ColorR<uint>, uint>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16_FLOAT:
					frames.Add(new ImageBuffer<ColorRG<Half>, Half>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16_UNORM:
				case DXGIFormat.R16G16_UINT:
					frames.Add(new ImageBuffer<ColorRG<ushort>, ushort>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16_SNORM:
				case DXGIFormat.R16G16_SINT:
					frames.Add(new ImageBuffer<ColorRG<short>, short>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16B16A16_FLOAT:
					frames.Add(new ImageBuffer<ColorRGBA<Half>, Half>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16B16A16_SINT:
				case DXGIFormat.R16G16B16A16_SNORM:
					frames.Add(new ImageBuffer<ColorRGBA<short>, short>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16B16A16_UINT:
				case DXGIFormat.R16G16B16A16_UNORM:
					frames.Add(new ImageBuffer<ColorRGBA<ushort>, ushort>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32_FLOAT:
					frames.Add(new ImageBuffer<ColorRG<float>, float>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32_SINT:
					frames.Add(new ImageBuffer<ColorRG<int>, int>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32_UINT:
					frames.Add(new ImageBuffer<ColorRG<uint>, uint>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32B32A32_FLOAT:
					frames.Add(new ImageBuffer<ColorRGBA<float>, float>(chunk, width, height));
					continue;
				case DXGIFormat.B8G8R8A8_UNORM:
				case DXGIFormat.B8G8R8A8_UNORM_SRGB:
					frames.Add(new ImageBuffer<Color<byte, B, G, R, A>, byte>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8B8_UNORM:
					frames.Add(new ImageBuffer<ColorRGB<byte>, byte>(chunk, width, height));
					continue;
				default:
					throw new NotSupportedException();
			}
		}

		if (flags.BlenderSafe && (isMetalGlossMap || isNormalMap)) {
			for (var index = 0; index < frames.Count; index++) {
				var image = frames[index];
				var oldImage = default(IImageBuffer);
				ImageBuffer<ColorRGBA<byte>, byte> newImage;
				if (image is ImageBuffer<ColorRGBA<byte>, byte> rgba) {
					newImage = rgba;
				} else {
					oldImage = image;
					newImage = (ImageBuffer<ColorRGBA<byte>, byte>) image.Cast<ColorRGBA<byte>, byte>();
					frames[index] = newImage;
				}

				foreach (ref var pixel in newImage.ColorData.Memory.Span[..(newImage.Width * newImage.Height)]) {
					if (isMetalGlossMap) {
						var roughness = (byte) (0xFF - pixel.R);
						var metalness = pixel.B;
						pixel = new ColorRGBA<byte>(pixel.G, roughness, metalness, pixel.A);
					} else if (isNormalMap) {
						// why
						pixel.R ^= 0xFF;
						pixel.G ^= 0xFF;
						pixel.A = pixel.B; // waterline.
						pixel.B = 0xFF;
					}
				}

				oldImage?.Dispose();
			}
		}

		if (!isCubemap) {
			return SaveTexture(path, flags, encoder, frames);
		}

		using var cubemap = new IBLImage(frames, CubemapOrder.DXGIOrder);
		using var converted = cubemap.Convert(flags.CubemapStyle);
		return SaveTexture(path, flags, encoder, [converted]);
	}

	[MethodImpl(MethodConstants.Optimize)]
	public static string SaveTexture(string path, IConversionOptions flags, IEncoder encoder, ImageCollection frames) {
		if (flags.Dry) {
			return path;
		}

		using var stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
		encoder.Write(stream, EncoderWriteOptions.Default, frames);
		return path;
	}

	[MethodImpl(MethodConstants.Optimize)]
	public static int CreateTexture(ModelBuilderContext context, Root gltf, ResourceId id, StringId slotName) {
		AkizukiLog.Information("Converting texture {Path}", id);
		if (context.TextureCache.TryGetValue(id, out var texId)) {
			return texId;
		}

		var texturePkgPath = id.Path;
		var isDDS = texturePkgPath.EndsWith(".dds");
		if (isDDS) {
			for (var hdIndex = 0; hdIndex < 2; ++hdIndex) {
				var hdPath = id.Path[..^1] + hdIndex.ToString("D");
				if (!context.Manager.HasFile(hdPath)) {
					continue;
				}

				texturePkgPath = hdPath;
				break;
			}
		}

		using var buffer = context.Manager.OpenFile(texturePkgPath);
		if (buffer == null) {
			return context.TextureCache[id] = -1;
		}

		var texturePath = Path.Combine(context.TexturesPath, $"{id.Hash:x16}-" + Path.GetFileName(texturePkgPath));
		if (context.Flags.SelectedFormat is not TextureFormat.None && isDDS) {
			if (ConvertTexture(texturePath, context.Flags, buffer, true, slotName == NormalMapId, slotName == MetalMapId) is not { } newTexturePath) {
				return context.TextureCache[id] = -1;
			}

			texturePath = Path.GetRelativePath(context.ModelPath, newTexturePath);
		} else if (!context.Flags.Dry) {
			using var stream = new FileStream(texturePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
			stream.Write(buffer.Span);
		}

		return context.TextureCache[id] = gltf.CreateTexture(slotName.Text, texturePath, WrapMode.Repeat, WrapMode.Repeat, null, null).Id;
	}
}
