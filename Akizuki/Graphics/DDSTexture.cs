// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Akizuki.Structs.Graphics;
using DragonLib.IO;

namespace Akizuki.Graphics;

public sealed class DDSTexture : IDisposable {
	public DDSTexture(IMemoryBuffer<byte> data) {
		Buffer = data;
		var reader = new MemoryReader(data);

		var header = reader.Read<DDSHeader>();
		StartOffset = header.Size + 4;
		Width = header.Width;
		Height = header.Height;
		Mips = (header.Caps & DDSCaps.MipMapped) != 0 ? header.MipMapCount : 1;
		Surfaces = 1;

		switch (header.PixelFormat.FourCC) {
			case 0x31545844: Format = DXGIFormat.BC1_UNORM; break;
			case 0x32545844 or 0x33545844: Format = DXGIFormat.BC2_UNORM; break;
			case 0x34545844 or 0x35545844: Format = DXGIFormat.BC3_UNORM; break;
			case 0x31495441 or 0x55344342: Format = DXGIFormat.BC4_UNORM; break;
			case 0x53344342: Format = DXGIFormat.BC4_SNORM; break;
			case 0x32495441 or 0x55354342: Format = DXGIFormat.BC5_UNORM; break;
			case 0x53354342: Format = DXGIFormat.BC5_SNORM; break;
			case 0x30315844: {
				var dx10 = reader.Read<DX10Header>();
				StartOffset += Unsafe.SizeOf<DX10Header>();
				Surfaces = dx10.ArraySize;
				Debug.Assert(dx10.ResourceDimension == DXGIResourceDimension.Texture2D);
				Format = dx10.Format;
				break;
			}
			case var _ when header.PixelFormat.FourCC > 0: Format = (DXGIFormat) header.PixelFormat.FourCC; break;
			default: {
				if ((header.PixelFormat.Flags & (DDSPixelFormatFlags.RGB | DDSPixelFormatFlags.Luminance)) == 0) {
					throw new NotSupportedException();
				}

				switch (header.PixelFormat.RGBBitCount) {
					case 8:
						Format = DXGIFormat.R8_UNORM;
						break;
					case 16:
						Format = DXGIFormat.R8G8_UNORM;
						break;
					case 24:
						Format = DXGIFormat.R8G8B8_UNORM;
						break;
					case 32:
						Format = DXGIFormat.R8G8B8A8_UNORM;
						break;
					default:
						throw new NotSupportedException();
				}

				break;
			}
		}

		if ((header.Caps2 & DDSCaps2.CubeMapAll) == DDSCaps2.CubeMapAll) {
			Surfaces *= 6;
		}

		OneSurfaceSize = (int) CalculateSurfaceSize(out var oneMipSize);
		OneMipSize = (int) oneMipSize;
	}

	private IMemoryBuffer<byte> Buffer { get; }
	public int Width { get; }
	public int Height { get; }
	public int Surfaces { get; }
	public int Mips { get; }
	public int OneSurfaceSize { get; }
	public int OneMipSize { get; }
	private int StartOffset { get; }
	public DXGIFormat Format { get; }

	private (uint BitsPerBlock, uint PixelsPerBlock) PitchFactor =>
		Format switch {
			DXGIFormat.BC1_UNORM or DXGIFormat.BC1_UNORM_SRGB => (64, 16),
			DXGIFormat.BC2_UNORM or DXGIFormat.BC2_UNORM_SRGB or DXGIFormat.BC3_UNORM or DXGIFormat.BC3_UNORM_SRGB => (128, 16),
			DXGIFormat.BC4_UNORM or DXGIFormat.BC4_SNORM => (64, 16),
			DXGIFormat.BC5_UNORM or DXGIFormat.BC5_SNORM or DXGIFormat.BC6H_SF16 or DXGIFormat.BC6H_UF16 or DXGIFormat.BC7_UNORM or DXGIFormat.BC7_UNORM_SRGB => (128, 16),
			DXGIFormat.IA44 or DXGIFormat.P8 or DXGIFormat.A8_UNORM or DXGIFormat.R8_UNORM or DXGIFormat.R8_SNORM => (8, 1),
			DXGIFormat.A8P8 or DXGIFormat.R8G8_SINT or DXGIFormat.R8G8_UINT or DXGIFormat.R8G8_SNORM or DXGIFormat.R8G8_UNORM or DXGIFormat.R16_SINT or DXGIFormat.R16_UINT or DXGIFormat.R16_FLOAT or DXGIFormat.R16_SNORM or DXGIFormat.R16_UNORM => (16, 1),
			DXGIFormat.R8G8B8_UNORM => (24, 1),
			DXGIFormat.R8G8B8A8_UNORM or DXGIFormat.R8G8B8A8_UNORM_SRGB or DXGIFormat.R8G8B8A8_SNORM or DXGIFormat.R8G8B8A8_SINT or DXGIFormat.R8G8B8A8_UINT or DXGIFormat.R32_FLOAT or DXGIFormat.R32_SINT or DXGIFormat.R32_UINT or DXGIFormat.R16G16_FLOAT or DXGIFormat.R16G16_UNORM or DXGIFormat.R16G16_UINT or DXGIFormat.R16G16_SNORM or DXGIFormat.R16G16_SINT or DXGIFormat.R10G10B10A2_UNORM or DXGIFormat.R10G10B10A2_UINT or DXGIFormat.B8G8R8A8_UNORM or DXGIFormat.B8G8R8A8_UNORM_SRGB
				or DXGIFormat.B8G8R8X8_UNORM or DXGIFormat.B8G8R8X8_UNORM_SRGB => (32, 1),
			DXGIFormat.R16G16B16A16_FLOAT or DXGIFormat.R16G16B16A16_SINT or DXGIFormat.R16G16B16A16_UINT or DXGIFormat.R16G16B16A16_SNORM or DXGIFormat.R16G16B16A16_UNORM or DXGIFormat.R32G32_FLOAT or DXGIFormat.R32G32_SINT or DXGIFormat.R32G32_UINT => (64, 1),
			DXGIFormat.R32G32B32A32_FLOAT => (128, 1),
			_ => (0, 0),
		};

	public void Dispose() => Buffer.Dispose();

	public IMemoryBuffer<byte> GetSurface(int index) => new BorrowedMemoryBuffer<byte>(Buffer, OneMipSize, StartOffset + OneSurfaceSize * index);

	private uint CalculateSurfaceSize(out uint largestMip) {
		var (bitsPerBlock, pixelsPerBlock) = PitchFactor;
		if (pixelsPerBlock == 0 || bitsPerBlock == 0) {
			throw new NotSupportedException();
		}

		var oneSurface = 0u;
		// this will always work as long as width and height are stable powers of 2, you're welcome
		var mask = ((uint) Width * (uint) Height / pixelsPerBlock * bitsPerBlock) >> 3;
		largestMip = mask;
		for (var i = 0; i < Mips; ++i) {
			oneSurface ^= mask; // maybe use += instead of ^= for non-power-of-2?
			mask >>= 2;
		}

		return oneSurface;
	}
}
