// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using Akizuki.Structs.Data;
using DragonLib.Hash.Algorithms;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class EffectMetadataPrototype : IPrototype {
	static EffectMetadataPrototype() {
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

	public static IPrototype Create(MemoryReader reader, BigWorldDatabase db) => new EffectMetadataPrototype(reader);

	public static void AppendVersion(StringBuilder sb) {
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 4, "intensitiesCount");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, nestedKind: BWDBFieldKind.Type);

		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 4, "size");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, 1, "text", BWDBFieldKind.Signed);
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 4, "size");
		sb.Append("intensityName");

		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Float, 4, "minIntensity");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Float, 4, "maxIntensity");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Float, 4, "defaultIntensity");

		sb.Append("intensitiesMetadata");
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Unsigned, 4, "intensitiesCount");
	}

	public static uint Version { get; }
	public static uint Id { get; } = MurmurHash3Algorithm.Hash32_32("EffectMetadataPrototype"u8);
	public static string PrototypeName => "EffectMetadataPrototype";
	public static int Size => 0x10;
}
