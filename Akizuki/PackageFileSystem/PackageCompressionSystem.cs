// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Akizuki.PackageFileSystem;

public enum PackageCompressionSystem : uint {
	None = 0,
	// the question is what is 1..4 lol
	Block = 5, // WoWS / WoWSL
	TileStream = 6, // MK
}
