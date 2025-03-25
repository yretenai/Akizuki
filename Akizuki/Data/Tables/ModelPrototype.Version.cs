// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Text;
using Akizuki.Structs.Data;
using Akizuki.Structs.Data.Tables;
using DragonLib.Hash.Algorithms;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class ModelPrototype : IPrototype {
	static ModelPrototype() {
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

	public static IPrototype Create(MemoryReader reader, BigWorldDatabase db) => new ModelPrototype(reader, db);

	public static void AppendVersion(StringBuilder sb) {
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type, 8, "visualResourceId");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.None, 0, "miscType");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "animationsCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 8, "animations", BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 1, "animationsCount");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "dyesCount");

		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, nestedKind: BWDBFieldKind.Type);
		DyePrototype.AppendVersion(sb);
		sb.Append("dyes");

		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 1, "dyesCount");
	}

	public static uint Version { get; }
	public static uint Id { get; } = MurmurHash3Algorithm.Hash32_32("ModelPrototype"u8);
	public static string PrototypeName => "ModelPrototype";
	public static int Size => Unsafe.SizeOf<ModelPrototypeHeader>();
}
