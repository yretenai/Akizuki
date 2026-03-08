// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Concurrent;
using Waterfall.Hash.Algorithms;

namespace Akizuki.Moo;

public record struct StringId(uint Hash) {
	static StringId() {
		Lookup["MaterialPrototype"] = "MaterialPrototype";
		Lookup["VisualPrototype"] = "VisualPrototype";
		Lookup["ModelPrototype"] = "ModelPrototype";
		Lookup["SkeletonPrototype"] = "SkeletonPrototype";
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
	public override string ToString() => $"0x{Hash} ({Lookup.GetValueOrDefault(this, "unknown")})";
	public static implicit operator StringId(uint hash) => new(hash);
	public static implicit operator StringId(string path) => new(path);
	public static implicit operator uint(StringId id) => id.Hash;
	public static implicit operator string(StringId id) => Lookup.GetValueOrDefault(id) ?? $"0x{id.Hash}";
}
