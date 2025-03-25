// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data.Tables;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct DyePrototypeHeader {
	public StringId MatterId { get; set; }
	public StringId ReplacesId { get; set; }
	public int TintCount { get; set; }
	public long TintNameIdsPtr { get; set; }
	public long TintMaterialIdsPtr { get; set; }
}
