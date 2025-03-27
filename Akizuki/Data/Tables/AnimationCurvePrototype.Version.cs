// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Text;
using Akizuki.Structs.Data;

namespace Akizuki.Data.Tables;

public partial class AnimationCurvePrototype<T> {
	public static void AppendVersion(StringBuilder sb) {
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Float, 4, "period");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Unsigned, 1, "repeated");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Signed, 4, "count");
		IPrototype.AppendField(sb, BWDBFieldType.Array, BWDBFieldKind.Pointer, nestedKind: BWDBFieldKind.Type);
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Float, 4, "time");
		IPrototype.AppendField(sb, BWDBFieldType.Field, typeof(T) == typeof(float) ? BWDBFieldKind.Float : BWDBFieldKind.Type, Unsafe.SizeOf<T>(), "value");
		sb.Append("points");
		IPrototype.AppendField(sb, BWDBFieldType.Count, BWDBFieldKind.Signed, 4, "count");
		sb.Append("ramp");
	}
}
