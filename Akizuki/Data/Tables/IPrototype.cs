// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using System.Text.Json.Serialization;
using Akizuki.Structs.Data;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

[JsonPolymorphic, JsonDerivedType(typeof(MaterialPrototype))]
public interface IPrototype {
	public static virtual uint Version => 0;
	public static virtual uint Id => 0;
	public static virtual int Size => 0;
	public static virtual string Name => string.Empty;
	public static virtual void AppendVersion(StringBuilder sb) { }
	public static virtual IPrototype Create(MemoryReader data, BigWorldDatabase db) => throw new NotSupportedException();

	internal static void AppendField(StringBuilder builder, BWDBFieldType type, BWDBFieldKind kind, int size = 0, string? name = null, BWDBFieldKind nestedKind = BWDBFieldKind.None) {
		builder.Append((char) type);
		builder.Append((char) kind);
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
