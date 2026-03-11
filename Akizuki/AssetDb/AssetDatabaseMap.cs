// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using DragonLib.IO.Binary;

namespace Akizuki.AssetDb;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct AssetDatabaseMap {
	public long Count { get; set; }
	public long KeyOffset { get; set; }
	public long ValueOffset { get; set; }

	public Dictionary<TKey, TValue> Read<TKey, TValue>(long structOffset, BufferBinaryReader reader) where TKey : notnull {
		throw new NotImplementedException();
	}
}
