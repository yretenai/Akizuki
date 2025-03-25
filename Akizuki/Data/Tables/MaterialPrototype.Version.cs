// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using Akizuki.Structs.Data;
using DragonLib.Hash.Algorithms;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class MaterialPrototype : IPrototype {
	static MaterialPrototype() {
		var sb = new StringBuilder(0x4000);
		sb.Append(PrototypeName);
		AppendVersion(sb);

		Span<byte> version = stackalloc byte[0x4000];
		var offset = 0;
		foreach (var chunk in sb.GetChunks()) {
			offset += Encoding.ASCII.GetBytes(chunk.Span, version[offset..]);
		}

		Version = MurmurHash3Algorithm.Hash32_32(version[..offset]);
	}

	public static IPrototype Create(MemoryReader reader, BigWorldDatabase db) => new MaterialPrototype(reader, db);

	public static void AppendVersion(StringBuilder sb) {
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 2, "propertiesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "boolValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "intValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "uintValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "floatValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "textureValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "vector2ValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "vector3ValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "vector4ValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "matrixValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 4, "propertyNameIds", BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 2, "propertiesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 2, "propertyIds", BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 2, "propertiesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 1, "boolValues", BWDBFieldKind.Unsigned);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 1, "boolValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 4, "intValues", BWDBFieldKind.Signed);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 1, "intValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 4, "uintValues", BWDBFieldKind.Unsigned);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 1, "uintValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 4, "floatValues", BWDBFieldKind.Float);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 1, "floatValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 8, "textureValues", BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 1, "textureValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 8, "vector2Values", BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 1, "vector2ValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 12, "vector3Values", BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 1, "vector3ValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 16, "vector4Values", BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 1, "vector4ValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 64, "matrixValues", BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 1, "matrixValuesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type, 8, "fxPathId");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Signed, 4, "collisionFlags");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Signed, 4, "sortOrder");
	}

	public static uint Version { get; }
	public static uint Id { get; } = MurmurHash3Algorithm.Hash32_32("MaterialPrototype"u8);
	public static string PrototypeName => "MaterialPrototype";
	public static int Size => 0x78;
}
