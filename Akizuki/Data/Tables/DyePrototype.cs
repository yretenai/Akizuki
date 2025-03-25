// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Data.Tables;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class DyePrototype {
	public DyePrototype(DyePrototypeHeader header, MemoryReader data, BigWorldDatabase db) {
		var offset = data.Offset;

		Matter = db.GetString(header.MatterId);
		Replaces = db.GetString(header.ReplacesId);

		data.Offset = (int) (offset + header.TintNameIdsPtr);
		var names = data.Read<uint>(header.TintCount);
		for (var index = 0; index < header.TintCount; ++index) {
			Tints.Add(db.GetString(names[index]));
		}

		data.Offset = (int) (offset + header.TintMaterialIdsPtr);
		var ids = data.Read<ulong>(header.TintCount);
		for (var index = 0; index < header.TintCount; ++index) {
			Materials.Add(db.GetPath(ids[index]));
		}
	}

	public string Matter { get; set; }
	public string Replaces { get; set; }
	public List<string> Tints { get; set; } = [];
	public List<string> Materials { get; set; } = [];
}
