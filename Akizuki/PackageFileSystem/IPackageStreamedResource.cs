// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;

namespace Akizuki.PackageFileSystem;

public interface IPackageStreamedResource<out TPointer> where TPointer : INumber<TPointer> {
	TPointer Offset { get; }
	TPointer Size { get; }
	int CompressedSize { get; }
	PackageCompressionSystem CompressionSystem { get; }
	PackageCompressionType CompressionType { get; }
	ushort CompressionVersion { get; }
	uint Checksum { get; }
}
