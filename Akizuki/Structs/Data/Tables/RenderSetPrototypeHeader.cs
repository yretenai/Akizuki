// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data.Tables;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct RenderSetPrototypeHeader {
	public StringId NameId { get; set; }
	public StringId MaterialNameId { get; set; }
	public StringId VerticesName { get; set; }
	public StringId IndicesName { get; set; }
	public ResourceId MaterialResourceId { get; set; }
	public bool IsSkinned { get; set; }
	public byte NodeCount { get; set; }
	public long NodeNameIdsPtr { get; set; }
}
