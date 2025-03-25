// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Text;
using Akizuki.Structs.Data;
using Akizuki.Structs.Data.Tables;
using DragonLib.Hash.Algorithms;

namespace Akizuki.Data.Tables;

public partial class RenderSetPrototype : IPrototype {
	static RenderSetPrototype() {
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
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type, 4, "nameId");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type, 4, "materialNameId");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type, 8, "materialMfmId");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "skinned");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "nodesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 4, "nodeNameIds", BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 1, "nodesCount");
	}

	public static uint Version { get; }
	public static uint Id { get; } = MurmurHash3Algorithm.Hash32_32("RenderSetPrototype"u8);
	public static string PrototypeName => "RenderSetPrototype";
	public static int Size => Unsafe.SizeOf<RenderSetPrototypeHeader>();
}
