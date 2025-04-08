// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct DDSPixelFormat() {
	public uint Size { get; set; } = 32;
	public DDSPixelFormatFlags Flags { get; set; } = DDSPixelFormatFlags.Identifier;
	public D3DFORMAT Identifier { get; set; }
	public uint RGBBitCount { get; set; }
	public uint RBitMask { get; set; }
	public uint GBitMask { get; set; }
	public uint BBitMask { get; set; }
	public uint ABitMask { get; set; }
}
