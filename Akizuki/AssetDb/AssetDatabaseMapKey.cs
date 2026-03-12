// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.AssetDb;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct AssetDatabaseMapKey<T, TBucket> where T : struct where TBucket : struct {
	public T Key { get; set; }
	public TBucket Bucket { get; set; }
}

public interface IAssetDatabaseMapBucket {
	bool IsValid { get; }
	int Id { get; }
}

public record struct AssetDatabaseMapBucket : IAssetDatabaseMapBucket {
	public uint BucketId { get; set; }

	public bool IsValid => (BucketId & 0x80000000) != 0;
	public int Id => (int) (BucketId & 0x7FFFFFFF);
}

public record struct AssetDatabaseMapBucket64 : IAssetDatabaseMapBucket {
	public uint Padding { get; set; }
	public uint BucketId { get; set; }

	public bool IsValid => (BucketId & 0x80000000) != 0;
	public int Id => (int) (BucketId & 0x7FFFFFFF);
}
