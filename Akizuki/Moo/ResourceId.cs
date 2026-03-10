// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Waterfall.Hash.Algorithms;

namespace Akizuki.Moo;

[StructLayout(LayoutKind.Explicit, Size = 8)] [DebuggerDisplay("{" + nameof(ToDebugString) + "()}")]
public readonly record struct ResourceId(
	[field: FieldOffset(0)]
	ulong Hash) : IComparable, IComparable<ResourceId> {
	public ResourceId(string path) : this(CityHashAlgorithm.Hash64(path)) { }

	public static ConcurrentDictionary<ResourceId, string> Lookup { get; } = new() {
		[0xDBB1A1D1B108B927ul] = "res",
	};

	public bool IsValid => Hash is > 0 and < 0xffffffffffffffff;
	public override string ToString() => Lookup.GetValueOrDefault(this) ?? $"0x{Hash:x}";

	public string ToDebugString() => $"\"{Lookup.GetValueOrDefault(this, "<unknown>")}\" (0x{Hash:x})";
	public static implicit operator ResourceId(ulong hash) => new(hash);
	public static implicit operator ResourceId(string path) => new(path);
	public static implicit operator ulong(ResourceId id) => id.Hash;
	public static implicit operator string(ResourceId id) => id.ToString();
	public int CompareTo(object? obj) => obj is ResourceId other ? CompareTo(other) : 0;
	public int CompareTo(ResourceId other) => Hash.CompareTo(other.Hash);
	public override int GetHashCode() => Hash.GetHashCode();
}
