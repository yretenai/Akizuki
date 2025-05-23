// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using System.Text.Json.Serialization;
using Akizuki.Structs.Data;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

[JsonPolymorphic]
[JsonDerivedType(typeof(MaterialPrototype))]
[JsonDerivedType(typeof(VisualPrototype))]
[JsonDerivedType(typeof(ModelPrototype))]
[JsonDerivedType(typeof(PointLightPrototype))]
[JsonDerivedType(typeof(VelocityFieldPrototype))]
[JsonDerivedType(typeof(AtlasContourPrototype))]
[JsonDerivedType(typeof(EffectPrototype))]
[JsonDerivedType(typeof(EffectPresetPrototype))]
[JsonDerivedType(typeof(EffectMetadataPrototype))]
public interface IPrototype {
	public static virtual uint Version => 0;
	public static virtual uint Id => 0;
	public static virtual int Size => 0;
	public static virtual string PrototypeName => string.Empty;
	public static virtual void AppendVersion(StringBuilder sb) { }
	public static virtual IPrototype Create(MemoryReader data) => throw new NotSupportedException();

	internal static void AppendField(StringBuilder builder, BWDBFieldType type, BWDBFieldKind kind, int size = 0, string? name = null, BWDBFieldKind nestedKind = BWDBFieldKind.None) {
		builder.Append((char) type);

		if (kind != BWDBFieldKind.None) {
			builder.Append((char) kind);
		}

		if (nestedKind != BWDBFieldKind.None) {
			builder.Append((char) nestedKind);
		}

		if (size > 0) {
			builder.Append(size);
		}

		if (name != null) {
			builder.Append(name);
		}
	}
}
