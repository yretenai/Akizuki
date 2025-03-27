// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Text;
using Akizuki.Structs.Data;
using Akizuki.Structs.Data.Tables;
using DragonLib.Hash.Algorithms;

namespace Akizuki.Data.Tables;

public partial class DyePrototype {
	static DyePrototype() {
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

	public static uint Version { get; }
	public static uint Id { get; } = MurmurHash3Algorithm.Hash32_32("DyePrototype"u8);
	public static string PrototypeName => "DyePrototype";
	public static int Size => Unsafe.SizeOf<DyePrototypeHeader>();

	public static void AppendVersion(StringBuilder sb) {
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type, 4, "matterId");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type, 4, "replacesId");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 4, "tintsCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 4, "tintNameIds", BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 4, "tintsCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 8, "tintMaterialIds", BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 4, "tintsCount");
	}
}
