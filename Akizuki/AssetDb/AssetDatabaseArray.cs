// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using DragonLib.IO.Binary;

namespace Akizuki.AssetDb;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct AssetDatabaseArray {
	public long Count { get; set; }
	public long Offset { get; set; }

	public List<T> Read<T>(long structOffset, BufferBinaryReader reader) {
		throw new NotImplementedException();
	}
}
