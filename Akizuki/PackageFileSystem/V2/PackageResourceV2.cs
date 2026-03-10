// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using System.Runtime.InteropServices;
using Akizuki.Moo;

namespace Akizuki.PackageFileSystem.V2;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct PackageResourceV2<TPointer> where TPointer : INumber<TPointer> {
	public ResourceId Id { get; set; }
	public ResourceId StreamId { get; set; }
	public TPointer Offset { get; set; }
	public PackageCompressionType CompressionDecoder { get; set; }
	public PackageCompressionType CompressionEncoder { get; set; }
	public int CompressedSize { get; set; }
	public uint Checksum { get; set; }
	public TPointer Size { get; set; }

	// Note on CompressionDecoder and CompressionEncoder, these used to be called Flags and CompressionType respectively.
	// None always is true if either is None, regardless of the other type.
	// Seen pairs:
	// Deflate, Deflate => Deflate
	// Deflate, None => None
	// OodleSelkie, None => None (MK)
	// None, Deflate => None (MK)
	// None, OodleMermaid => None (MK)
	// OodleKraken, OodleHydra => Same execution path (MK)
	//
	// DeflateBlocks is not actually seen, maybe an pre-oodle MK release? Engine still tries to load a DataStream header and ends up calling zlib.

	public override int GetHashCode() => Id.GetHashCode();
}
