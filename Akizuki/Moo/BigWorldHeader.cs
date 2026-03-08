// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Moo;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct BigWorldHeader {
	public uint Magic { get; set; }
	public BigWorldVersion Version { get; set; }
	public uint FileChecksum { get; set; }
	public int PointerSize { get; set; }
}
