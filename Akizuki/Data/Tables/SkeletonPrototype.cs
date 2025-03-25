// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Data.Tables;
using DragonLib.IO;
using Silk.NET.Maths;

namespace Akizuki.Data.Tables;

public partial class SkeletonPrototype {
	public SkeletonPrototype(SkeletonPrototypeHeader header, MemoryReader reader, BigWorldDatabase db) {
		var offset = reader.Offset;

		// reader.Offset = (int) (offset + header.NameMapNameIdsPtr);
		// var nameMapNameIds = reader.Read<uint>(header.NodeCount);
		reader.Offset = (int) (offset + header.NameMapNodeIdsPtr);
		var nameMapNodeIds = reader.Read<ushort>(header.NodeCount);
		reader.Offset = (int) (offset + header.NameIdsPtr);
		var nameIds = reader.Read<uint>(header.NodeCount);
		reader.Offset = (int) (offset + header.MatricesPtr);
		var matrices = reader.Read<Matrix4X4<float>>(header.NodeCount);
		reader.Offset = (int) (offset + header.ParentIdsPtr);
		var parentIds = reader.Read<ushort>(header.NodeCount);

		for (var index = 0; index < header.NodeCount; ++index) {
			// NameMap.Add(db.GetString(nameMapNameIds[index]));
			IdMap.Add(nameMapNodeIds[index]);
			Names.Add(db.GetString(nameIds[index]));
			Matrices.Add(matrices[index]);
			ParentIds.Add(parentIds[index]);
		}
	}

	// public List<string> NameMap { get; set; } = [];
	public List<ushort> IdMap { get; set; } = [];
	public List<string> Names { get; set; } = [];
	public List<Matrix4X4<float>> Matrices { get; set; } = [];
	public List<ushort> ParentIds { get; set; } = [];
}
