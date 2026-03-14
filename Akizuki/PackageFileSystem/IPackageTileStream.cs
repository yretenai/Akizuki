// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;

namespace Akizuki.PackageFileSystem;

public interface IPackageTileStream<out TPointer> where TPointer : INumber<TPointer> {
	TPointer DataOffset { get; }
	int CompressionLevel { get; }
	uint CompressionType { get; }
	TPointer CompressedSize { get; }
	TPointer Size { get; }
	int BlockCount { get; }
	int BlockSize { get; }
}
