// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;

namespace Akizuki.PackageFileSystem;

public interface IPackageStreamedResource<TPointer> where TPointer : INumber<TPointer> {
	TPointer Offset { get; set; }
	TPointer Size { get; set; }
	int CompressedSize { get; set; }
	PackageCompressionSystem CompressionSystem { get; set; }
	PackageCompressionType CompressionType { get; set; }
	uint Checksum { get; set; }
}
