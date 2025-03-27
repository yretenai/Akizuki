// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Text;
using Akizuki.Structs.Data;
using Akizuki.Structs.Data.Tables;
using DragonLib.Hash.Algorithms;
using DragonLib.IO;
using Silk.NET.Maths;

namespace Akizuki.Data.Tables;

public partial class PointLightPrototype : IPrototype {
	static PointLightPrototype() {
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

	public static IPrototype Create(MemoryReader reader) => new PointLightPrototype(reader);

	public static void AppendVersion(StringBuilder sb) {
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type);
		AnimationCurvePrototype<Vector4D<float>>.AppendVersion(sb);
		sb.Append("colorAnimation");

		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type);
		AnimationCurvePrototype<float>.AppendVersion(sb);
		sb.Append("radiusAnimation");

		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type, 16, "color");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type, 12, "localPosition");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Float, 4, "radius");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 4, "minQuality");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "animatedColor");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "animatedRadius");
	}

	public static uint Version { get; }
	public static uint Id { get; } = MurmurHash3Algorithm.Hash32_32("PointLightPrototype"u8);
	public static string PrototypeName => "PointLightPrototype";
	public static int Size => Unsafe.SizeOf<PointLightPrototypeHeader>();
}
