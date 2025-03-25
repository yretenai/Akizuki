// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Text;
using Akizuki.Structs.Data;
using Akizuki.Structs.Data.Tables;
using DragonLib.Hash.Algorithms;

namespace Akizuki.Data.Tables;

public partial class SkeletonPrototype : IPrototype {
	static SkeletonPrototype() {
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

	public static void AppendVersion(StringBuilder sb) {
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Signed, 4, "nodesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 4, "nameMapNameIds", BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Signed, 4, "nodesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 2, "nameMapNodeIds", BWDBFieldKind.Signed);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Signed, 4, "nodesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 4, "nameIds", BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Signed, 4, "nodesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 64, "matrices", BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Signed, 4, "nodesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 2, "parentIds", BWDBFieldKind.Signed);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Signed, 4, "nodesCount");
	}

	public static uint Version { get; }
	public static uint Id { get; } = MurmurHash3Algorithm.Hash32_32("SkeletonPrototype"u8);
	public static string PrototypeName => "SkeletonPrototype";
	public static int Size => Unsafe.SizeOf<SkeletonPrototypeHeader>();
}
