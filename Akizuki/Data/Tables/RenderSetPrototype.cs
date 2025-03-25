// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Data.Tables;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class RenderSetPrototype {
	public RenderSetPrototype(RenderSetPrototypeHeader header, MemoryReader data, BigWorldDatabase db) {
		Name = db.GetString(header.NameId);
		MaterialName = db.GetString(header.MaterialNameId);
		IndicesName = db.GetString(header.IndicesName);
		VerticesName = db.GetString(header.VerticesName);
		MaterialResource = db.GetPath(header.MaterialResourceId);
		IsSkinned = header.IsSkinned;

		data.Offset += (int) header.NodeNameIdsPtr;
		var names = data.Read<uint>(header.NodeCount);
		for (var index = 0; index < header.NodeCount; ++index) {
			Nodes.Add(db.GetString(names[index]));
		}
	}

	public string Name { get; set; }
	public string MaterialName { get; set; }
	public string IndicesName { get; set; }
	public string VerticesName { get; set; }
	public string MaterialResource { get; set; }
	public bool IsSkinned { get; set; }
	public List<string> Nodes { get; set; } = [];
}
