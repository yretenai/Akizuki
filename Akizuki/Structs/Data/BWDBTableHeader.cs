// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct BWDBTableHeader {
	public uint Id { get; set; } // mmh3 of name
	public uint Hash { get; set; } // {name}({Tyep}{Field})*, i.e. FakePrototypeFucountAPfarray
	public long Count { get; set; }
	public long Ptr { get; set; }
}
