// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data.Tables;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct SkeletonPrototypeHeader {
	public int NodeCount { get; set; }
	public long NameMapNameIdsPtr { get; set; }
	public long NameMapNodeIdsPtr { get; set; }
	public long NameIdsPtr { get; set; }
	public long MatricesPtr { get; set; }
	public long ParentIdsPtr { get; set; }
}
