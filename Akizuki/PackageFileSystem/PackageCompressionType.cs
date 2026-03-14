// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Waterfall.Compression;

namespace Akizuki.PackageFileSystem;

public enum PackageCompressionType : ushort {
	None = 0,
	Deflate = 1,
	Block = 2,
	OodleKraken = 3,
	OodleLeviathan = 4,
	OodleMermaid = 5,
	OodleSelkie = 6,
	OodleHydra = 7,
}

public static class PackageCompressionExtensions {
	extension(PackageCompressionType type) {
		public CompressionType Waterfall => type switch {
			PackageCompressionType.None => CompressionType.None,
			PackageCompressionType.Deflate => CompressionType.Deflate,
			PackageCompressionType.Block => throw new NotSupportedException(),
			PackageCompressionType.OodleKraken or PackageCompressionType.OodleLeviathan or
				PackageCompressionType.OodleMermaid or PackageCompressionType.OodleSelkie or
				PackageCompressionType.OodleHydra => CompressionType.Oodle,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, default)
		};
	}
}
