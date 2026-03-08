// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Concurrent;
using Waterfall.Hash.Algorithms;

namespace Akizuki.Moo;

public record struct ResourceId(ulong Hash) {
	public ResourceId(string path) : this(CityHashAlgorithm.Hash64(path)) { }
	public static ConcurrentDictionary<ResourceId, string> Lookup { get; } = new();
	public bool IsValid => Hash is > 0 and < 0xffffffffffffffff;
	public override string ToString() => $"0x{Hash} ({Lookup.GetValueOrDefault(this, "unknown")})";
	public static implicit operator ResourceId(ulong hash) => new(hash);
	public static implicit operator ResourceId(string path) => new(path);
	public static implicit operator ulong(ResourceId id) => id.Hash;
	public static implicit operator string(ResourceId id) => Lookup.GetValueOrDefault(id) ?? $"0x{id.Hash}";
}
