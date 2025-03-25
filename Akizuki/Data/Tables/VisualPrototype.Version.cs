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

public partial class VisualPrototype : IPrototype {
	static VisualPrototype() {
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

	public static IPrototype Create(MemoryReader reader) => new VisualPrototype(reader);

	public static void AppendVersion(StringBuilder sb) {
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type);
		SkeletonPrototype.AppendVersion(sb);
		sb.Append("skeleton");

		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type, 8, "mergedGeometryPathId");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "underwaterModel");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "abovewaterModel");

		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type);
		BoundingBox.AppendVersion(sb);
		sb.Append("boundingBox");

		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 2, "renderSetsCount");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "lodsCount");

		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, nestedKind: BWDBFieldKind.Type);
		RenderSetPrototype.AppendVersion(sb);
		sb.Append("renderSets");
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 2, "renderSetsCount");

		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, nestedKind: BWDBFieldKind.Type);
		LODPrototype.AppendVersion(sb);
		sb.Append("lods");
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 1, "lodsCount");
	}

	public static uint Version { get; }
	public static uint Id { get; } = MurmurHash3Algorithm.Hash32_32("VisualPrototype"u8);
	public static string PrototypeName => "VisualPrototype";
	public static int Size => Unsafe.SizeOf<VisualPrototypeHeader>();
}
