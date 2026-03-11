// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.AssetDb;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct AssetDatabaseMapKey<T, TBucket> where T : unmanaged where TBucket : unmanaged {
	public T Value { get; set; }
	public TBucket Bucket { get; set; }
}
