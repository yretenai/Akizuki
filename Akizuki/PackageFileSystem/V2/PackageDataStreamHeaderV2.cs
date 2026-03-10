// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using System.Runtime.InteropServices;

namespace Akizuki.PackageFileSystem.V2;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct PackageDataStreamHeaderV2<TPointer> where TPointer : INumber<TPointer> {
	public TPointer DataOffset { get; set; }
	public uint CompressionDecoder { get; set; } // todo: fill this enum out
	public uint CompressionEncoder { get; set; } // assuming it's the same as PackageResourceV2 Decoder / Encoder.
	public TPointer CompressedSize { get; set; }
	public TPointer Size { get; set; }
	public int BlockCount { get; set; }
	public int BlockSize { get; set; }
	public ulong Reserved1 { get; set; } // pointer storage
	public ulong Reserved2 { get; set; }
}
