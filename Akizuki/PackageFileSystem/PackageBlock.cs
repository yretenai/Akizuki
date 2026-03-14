// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.PackageFileSystem;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
public record struct PackageBlock {
	public ushort Size { get; set; }
	public ushort IsCompressed { get; set; }
}
