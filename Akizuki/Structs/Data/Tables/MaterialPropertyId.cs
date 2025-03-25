// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data.Tables;

[StructLayout(LayoutKind.Sequential, Size = 2)]
public readonly record struct MaterialPropertyId(ushort Value) {
	public MaterialPropertyType Type => (MaterialPropertyType) (Value & 0xF);
	public int Index => Value >> 4;
}
