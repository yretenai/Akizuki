// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using Akizuki.Structs.Data.Tables;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class VisualPrototype {
	public VisualPrototype(MemoryReader data, BigWorldDatabase db) {
		var offset = data.Offset;
		var header = data.Read<VisualPrototypeHeader>();
		data.Offset = offset;
		Skeleton = new SkeletonPrototype(header.Skeleton, data, db);

		MergedGeometryPath = db.GetPath(header.MergedGeometryPathId);
		IsUnderwaterModel = header.IsUnderwaterModel;
		IsAbovewaterModel = header.IsAbovewaterModel;
		BoundingBox = header.BoundingBox;

		var lodOffset = data.Offset = (int) (offset + header.LODPtr);
		var lods = data.Read<LODPrototypeHeader>(header.LODCount);
		var oneLod = Unsafe.SizeOf<LODPrototypeHeader>();
		for (var index = 0; index < header.LODCount; ++index) {
			data.Offset = lodOffset;
			LOD.Add(new LODPrototype(lods[index], data, db));
			lodOffset += oneLod;
		}

		var renderSetOffset = data.Offset = (int) (offset + header.RenderSetsPtr);
		var renderSets = data.Read<RenderSetPrototypeHeader>(header.RenderSetsCount);
		var oneRenderSets = Unsafe.SizeOf<RenderSetPrototypeHeader>();
		for (var index = 0; index < header.RenderSetsCount; ++index) {
			data.Offset = renderSetOffset;
			RenderSets.Add(new RenderSetPrototype(renderSets[index], data, db));
			renderSetOffset += oneRenderSets;
		}
	}

	public SkeletonPrototype Skeleton { get; }
	public string MergedGeometryPath { get; }
	public bool IsUnderwaterModel { get; }
	public bool IsAbovewaterModel { get; }
	public BoundingBox BoundingBox { get; set; }
	public List<RenderSetPrototype> RenderSets { get; set; } = [];
	public List<LODPrototype> LOD { get; set; } = [];
}
