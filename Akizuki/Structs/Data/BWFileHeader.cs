using System.Buffers.Binary;
using System.Runtime.InteropServices;
using DragonLib;

namespace Akizuki.Structs.Data;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct BWFileHeader {
	public static uint PFSIMagic { get; } = BinaryPrimitives.ReverseEndianness("PFSI".AsMagicConstant<uint>());
	public static uint BWDBMagic { get; } = BinaryPrimitives.ReverseEndianness("BWDB".AsMagicConstant<uint>());
	public uint Magic { get; set; }
	public uint EndianTest { get; set; } // also doubles as version (in BE)
	public uint Hash { get; set; } // mmh3 of the data
	public uint Bits { get; set; } // 32 or 64
}
