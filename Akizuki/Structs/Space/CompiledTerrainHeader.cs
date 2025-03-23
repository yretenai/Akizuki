// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using DragonLib;

namespace Akizuki.Structs.Space;

public record struct CompiledTerrainHeader {
	public static uint TRBMagic { get; } = "trb".AsMagicConstant<uint>();

	public uint Magic { get; set; }
	public int Width { get; set; }
	public int Height { get; set; }
	public ushort Chunks { get; set; }
	public ushort UnitsPerChunk { get; set; }
	public float Min { get; set; }
	public float Max { get; set; }
}
