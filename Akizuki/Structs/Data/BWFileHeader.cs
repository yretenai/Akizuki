// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using DragonLib;

namespace Akizuki.Structs.Data;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct BWFileHeader {
	public static uint PFSIMagic { get; } = BinaryPrimitives.ReverseEndianness("PFSI".AsMagicConstant<uint>());
	public static uint BWDBMagic { get; } = BinaryPrimitives.ReverseEndianness("BWDB".AsMagicConstant<uint>());
	public uint Magic { get; set; }
	public uint SwappedVersion { get; set; }
	public uint Hash { get; set; } // mmh3 of the data
	public uint PointerSize { get; set; } // 32 or 64
	public int Version => (int) BinaryPrimitives.ReverseEndianness(SwappedVersion);
	public bool IsHostEndian => SwappedVersion > Version;
}
