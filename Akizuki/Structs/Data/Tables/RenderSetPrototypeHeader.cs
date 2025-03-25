// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data.Tables;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct RenderSetPrototypeHeader {
	public uint NameId { get; set; }
	public uint MaterialNameId { get; set; }
	public uint IndicesName { get; set; }
	public uint VerticesName { get; set; }
	public ulong MaterialMFMPathId { get; set; }
	public bool IsSkinned { get; set; }
	public byte NodeCount { get; set; }
	public long NodeNameIdsPtr { get; set; }
}
