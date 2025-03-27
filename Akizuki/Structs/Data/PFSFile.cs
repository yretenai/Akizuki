// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct PFSFile : IComparable<PFSFile>, IComparable<ulong> {
	public ulong Id { get; set; }
	public ulong PackageId { get; set; }
	public long Offset { get; set; }
	public PFSCompressionType CompressionType { get; set; }
	public PFSFileFlags Flags { get; set; }
	public int CompressedSize { get; set; }
	public uint Hash { get; set; } // crc32 of data
	public long UncompressedSize { get; set; }

	public bool Equals(PFSFile? other) => other?.Id == Id;

	public int CompareTo(PFSFile other) => Id.CompareTo(other.Id);
	public int CompareTo(ulong other) => Id.CompareTo(other);
}
