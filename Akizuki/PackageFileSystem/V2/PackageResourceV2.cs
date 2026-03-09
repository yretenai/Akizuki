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
	public ResourceId PackageId { get; set; }
	public TPointer Offset { get; set; }
	public uint Flags { get; set; }
	public PackageCompressionType CompressionType { get; set; }
	public int CompressedSize { get; set; }
	public uint Checksum { get; set; }
	public TPointer Size { get; set; }
}
