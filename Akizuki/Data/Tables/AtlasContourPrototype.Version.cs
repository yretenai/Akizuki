// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using Akizuki.Structs.Data;
using DragonLib.Hash.Algorithms;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class AtlasContourPrototype : IPrototype {
	static AtlasContourPrototype() {
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

	public static IPrototype Create(MemoryReader reader) => new AtlasContourPrototype(reader);

	public static void AppendVersion(StringBuilder sb) {
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Signed, 4, "frameCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, nestedKind: BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Signed, 4, "count");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, nestedKind: BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Float, 4, "x");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Float, 4, "y");
		sb.Append("points");
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Signed, 4, "count");
		sb.Append("contours");
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Signed, 4, "frameCount");
	}

	public static uint Version { get; }
	public static uint Id { get; } = MurmurHash3Algorithm.Hash32_32("AtlasContourProto"u8);
	public static string PrototypeName => "AtlasContourProto";
	public static int Size => 0x10;
}
