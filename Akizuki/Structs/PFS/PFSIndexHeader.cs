// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using DragonLib;

namespace Akizuki.Structs.PFS;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct PFSIndexHeader {
	public static uint PFSIMagic { get; } = BinaryPrimitives.ReverseEndianness("PFSI".AsMagicConstant<uint>());
	public uint Magic { get; set; }
	public uint EndianTest { get; set; }
	public uint Hash { get; set; } // mmh3 of the index data
	public uint Bits { get; set; } // 32 or 64
}
