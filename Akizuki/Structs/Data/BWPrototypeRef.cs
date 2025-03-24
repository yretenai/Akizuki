// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data;

[StructLayout(LayoutKind.Sequential, Size = 4)]
public readonly record struct BWPrototypeInfo(uint Value) {
	public int State => (int) (Value & 3);
	public int DatabaseIndex => (int) ((Value >> 2) & 0x3F);
	public int RecordIndex => (int) (Value >> 8);
	public bool IsValid => State == 0;
}
