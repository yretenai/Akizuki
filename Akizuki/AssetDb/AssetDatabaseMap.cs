// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DragonLib;
using DragonLib.IO.Binary;

namespace Akizuki.AssetDb;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct AssetDatabaseMap {
	public static int Size { get; } = Unsafe.SizeOf<AssetDatabaseMap>();

	public long Count { get; set; }
	public long KeyOffset { get; set; }
	public long ValueOffset { get; set; }

	public Dictionary<TKey, TValue> Read<TKey, TBucket, TValue>(long structOffset, BufferBinaryReader reader) where TKey : struct where TBucket : struct, IAssetDatabaseMapBucket where TValue : struct {
		var dict = ObjectPool<Dictionary<TKey, TValue>>.Rent();
		dict.Clear();
		dict.EnsureCapacity((int) Count);

		reader.Position = (int) (structOffset + KeyOffset);
		using var keys = reader.Read<AssetDatabaseMapKey<TKey, TBucket>>((int) Count);
		reader.Position = (int) (structOffset + ValueOffset);
		using var values = reader.Read<TValue>((int) Count);

		foreach (var (key, value) in keys.Zip(values)) {
			if (!key.Bucket.IsValid) {
				continue;
			}

			dict[key.Key] = value;
		}

		return dict;
	}
}
