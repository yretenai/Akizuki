// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data.Tables;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct VisualPrototypeHeader {
	public SkeletonPrototypeHeader Skeleton { get; set; }
	public ResourceId MergedGeometryPathId { get; set; }
	public bool IsUnderwaterModel { get; set; }
	public bool IsAbovewaterModel { get; set; }
	public ushort RenderSetsCount { get; set; }
	public byte LODCount { get; set; }
	public BoundingBox BoundingBox { get; set; }
	public long RenderSetsPtr { get; set; }
	public long LODPtr { get; set; }
}
