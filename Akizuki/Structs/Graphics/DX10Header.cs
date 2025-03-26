// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

namespace Akizuki.Structs.Graphics;

public record struct DX10Header() {
	public DXGIFormat Format { get; set; }
	public DXGIResourceDimension ResourceDimension { get; set; } = DXGIResourceDimension.Texture2D;
	public DX10Flags Flags { get; set; }
	public int ArraySize { get; set; }
	public DX10AlphaMode AlphaMode { get; set; }
}
