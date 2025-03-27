// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct GeometrySimpleBufferHeader {
	public long BufferLength { get; set; }
	public long NameLength { get; set; }
	public long NamePtr { get; set; }
	public long BufferPtr { get; set; }
}
