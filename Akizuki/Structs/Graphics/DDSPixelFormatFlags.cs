// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace Akizuki.Structs.Graphics;

[Flags]
public enum DDSPixelFormatFlags : uint {
	None = 0,
	AlphaPixels = 0x1,
	Alpha = 0x2,
	Identifier = 0x4,
	RGB = 0x40,
	YUV = 0x200,
	Luminance = 0x20000,
}
