// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Data;
using Akizuki.Structs.Data.Tables;
using DragonLib.IO;
using Silk.NET.Maths;

namespace Akizuki.Data.Tables;

public partial class SkeletonPrototype {
	public SkeletonPrototype(SkeletonPrototypeHeader header, MemoryReader reader) {
		var offset = reader.Offset;

		reader.Offset = (int) (offset + header.NameMapNameIdsPtr);
		var nameMapNameIds = reader.Read<StringId>(header.NodeCount);
		reader.Offset = (int) (offset + header.NameMapNodeIdsPtr);
		var nameMapNodeIds = reader.Read<ushort>(header.NodeCount);
		reader.Offset = (int) (offset + header.NameIdsPtr);
		var nameIds = reader.Read<StringId>(header.NodeCount);
		reader.Offset = (int) (offset + header.MatricesPtr);
		var matrices = reader.Read<Matrix4X4<float>>(header.NodeCount);
		reader.Offset = (int) (offset + header.ParentIdsPtr);
		var parentIds = reader.Read<ushort>(header.NodeCount);

		for (var index = 0; index < header.NodeCount; ++index) {
			NameMap[nameMapNameIds[index]] = nameMapNodeIds[index];
			Names.Add(nameIds[index]);
			Matrices.Add(matrices[index]);
			ParentIds.Add(parentIds[index]);
		}
	}

	public Dictionary<StringId, ushort> NameMap { get; set; } = [];
	public List<StringId> Names { get; set; } = [];
	public List<Matrix4X4<float>> Matrices { get; set; } = [];
	public List<ushort> ParentIds { get; set; } = [];
}
