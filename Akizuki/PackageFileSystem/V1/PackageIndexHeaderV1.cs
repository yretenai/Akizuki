// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using System.Runtime.InteropServices;

namespace Akizuki.PackageFileSystem.V1;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct PackageIndexHeaderV1<TPointer> where TPointer : INumber<TPointer> {
	public TPointer NameCount { get; set; }
	public TPointer NameTableOffset { get; set; }
	public TPointer ResourceCount { get; set; }
	public TPointer ResourceTableOffset { get; set; }
	public TPointer StreamCount { get; set; }
	public TPointer StreamTableOffset { get; set; }
}
