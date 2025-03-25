// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Data;
using Akizuki.Structs.Data.Tables;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class RenderSetPrototype {
	public RenderSetPrototype(RenderSetPrototypeHeader header, MemoryReader data) {
		Name = header.NameId;
		MaterialName = header.MaterialNameId;
		IndicesName = header.IndicesName;
		VerticesName = header.VerticesName;
		MaterialResource = header.MaterialResourceId;
		IsSkinned = header.IsSkinned;

		data.Offset += (int) header.NodeNameIdsPtr;
		var names = data.Read<StringId>(header.NodeCount);
		Nodes.AddRange(names);
	}

	public StringId Name { get; set; }
	public StringId MaterialName { get; set; }
	public StringId IndicesName { get; set; }
	public StringId VerticesName { get; set; }
	public ResourceId MaterialResource { get; set; }
	public bool IsSkinned { get; set; }
	public List<StringId> Nodes { get; set; } = [];
}
