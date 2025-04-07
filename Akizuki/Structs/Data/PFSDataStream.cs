// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct PFSDataStream {
	public long HeaderSize { get; set; }
	public int CompressionType { get; set; }
	public uint CompressionFlags { get; set; }
	public long UncompressedSize { get; set; }
	public long CompressedSize { get; set; }
	public int BlockCount { get; set; }
	public int BlockSize { get; set; }
	public long Reserved1 { get; set; }
	public long Reserved2 { get; set; }
}
