// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using System.Runtime.InteropServices;

namespace Akizuki.PackageFileSystem.V2;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct PackageIndexHeaderV2<TPointer> where TPointer : INumber<TPointer> {
	public int NameCount { get; set; }
	public int ResourceCount { get; set; }
	public int StreamCount { get; set; }
	public TPointer NameTableOffset { get; set; }
	public TPointer ResourceTableOffset { get; set; }
	public TPointer StreamTableOffset { get; set; }
}
