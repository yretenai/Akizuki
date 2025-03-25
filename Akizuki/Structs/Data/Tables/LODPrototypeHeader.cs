// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data.Tables;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct LODPrototypeHeader {
	public float Extent { get; set; }
	public bool CastsShadows { get; set; }
	public ushort RenderSetNamesCount { get; set; }
	public long RenderSetNamesPtr { get; set; }
}
