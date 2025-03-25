// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using System.Text;
using Akizuki.Data.Tables;
using Silk.NET.Maths;

namespace Akizuki.Structs.Data.Tables;

[StructLayout(LayoutKind.Explicit, Size = 32, Pack = 4)]
public record struct BoundingBox {
	[field: FieldOffset(0)] public Vector3D<float> Min { get; set; }
	[field: FieldOffset(16)] public Vector3D<float> Max { get; set; }

	public static void AppendVersion(StringBuilder sb) {
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type, 12, "min");
		IPrototype.AppendField(sb, BWDBFieldType.Field, BWDBFieldKind.Type, 12, "max");
	}
}
