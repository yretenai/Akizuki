// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Charon.Hash.Algorithms;

namespace Akizuki.Moo;

[StructLayout(LayoutKind.Explicit, Size = 4)] [DebuggerDisplay("{" + nameof(ToDebugString) + "()}")]
public readonly record struct StringId(
	[field: FieldOffset(0)]
	uint Hash) : IComparable, IComparable<StringId> {
	static StringId() {
		Lookup["MaterialPrototype"] = "MaterialPrototype";
		Lookup["VisualPrototype"] = "VisualPrototype";
		Lookup["ModelPrototype"] = "ModelPrototype";
		Lookup["SkeletonPrototype"] = "SkeletonPrototype";
		Lookup["SkeletonExtenderPrototype"] = "SkeletonExtenderPrototype";
		Lookup["PointLightPrototype"] = "PointLightPrototype";
		Lookup["AtlasContourProto"] = "AtlasContourProto";
		Lookup["EffectPrototype"] = "EffectPrototype";
		Lookup["TrailPrototype"] = "TrailPrototype";
		Lookup["MiscTypePrototype"] = "MiscTypePrototype";
		Lookup["MiscSettingsPrototype"] = "MiscSettingsPrototype";
		Lookup["VelocityFieldPrototype"] = "VelocityFieldPrototype";
		Lookup["EffectPresetPrototype"] = "EffectPresetPrototype";
		Lookup["EffectMetadataPrototype"] = "EffectMetadataPrototype";
		Lookup["RootPrototype"] = "RootPrototype";
	}

	public StringId(string path) : this(MurmurHash3Algorithm.Hash32_32(path)) { }
	public static ConcurrentDictionary<StringId, string> Lookup { get; } = new();
	public bool IsValid => Hash is > 0 and < 0xffffffff;
	public override string ToString() => Lookup.GetValueOrDefault(this) ?? $"0x{Hash:x}";
	public string ToDebugString() => $"\"{Lookup.GetValueOrDefault(this, "<unknown>")}\" (0x{Hash:x})";
	public static implicit operator StringId(uint hash) => new(hash);
	public static implicit operator StringId(string path) => new(path);
	public static implicit operator uint(StringId id) => id.Hash;
	public static implicit operator string(StringId id) => id.ToString();
	public int CompareTo(object? obj) => obj is StringId other ? CompareTo(other) : 0;
	public int CompareTo(StringId other) => Hash.CompareTo(other.Hash);
	public override int GetHashCode() => Hash.GetHashCode();
}
