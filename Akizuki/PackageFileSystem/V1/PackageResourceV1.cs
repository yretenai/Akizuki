// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using System.Runtime.InteropServices;
using Akizuki.Moo;

namespace Akizuki.PackageFileSystem.V1;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct PackageResourceV1<TPointer> : IPackageStreamedResource<TPointer> where TPointer : INumber<TPointer> {
	public TPointer Offset { get; set; }
	public int Reserved { get; set; } // 98% certain this is padding.
	public int CompressedSize { get; set; }
	public uint Checksum { get; set; }
	public int Size { get; set; }
	public PackageCompressionSystem CompressionSystem { get; set; }
	public PackageCompressionType CompressionType { get; set; }
	public ushort CompressionVersion { get; set; }
	public ResourceId Id { get; set; }
	public ResourceId StreamId { get; set; }

	TPointer IPackageStreamedResource<TPointer>.Size => TPointer.CreateTruncating(Size);

	public override int GetHashCode() => Id.GetHashCode();
}
