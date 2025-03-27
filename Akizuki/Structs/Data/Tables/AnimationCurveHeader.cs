// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data.Tables;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 32)]
public record struct AnimationCurveHeader {
	[field: FieldOffset(0)]
	public float Period { get; set; }

	[field: FieldOffset(4)]
	public bool Repeating { get; set; }

	[field: FieldOffset(16)]
	public CurveHeader Ramp { get; set; }
}
