// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Data.Tables;
using Akizuki.Structs.Data;
using DragonLib.IO;
using Serilog;

namespace Akizuki.Data;

public class BigWorldTable {
	public BigWorldTable(MemoryReader data, BWDBTableHeader header, BigWorldDatabase db) {
		Id = header.Id;
		Version = header.Hash;

		var info = data.Read<long>(2);
		var count = (int) info[0];
		var offset = (int) info[1];

		if (Id == MaterialPrototype.Id) {
			if (Version != MaterialPrototype.Version) {
				Log.Warning("Tried loading {Name} with an unsupported version!", MaterialPrototype.Name);
			}

			CreateRecords<MaterialPrototype>(data, count, offset, db);
		}
	}

	public uint Id { get; set; }
	public uint Version { get; set; }
	public List<IPrototype> Records { get; set; } = [];

	private void CreateRecords<T>(MemoryReader data, int count, int offset, BigWorldDatabase db) where T : IPrototype {
		for (var index = 0; index < count; ++index) {
			data.Offset = offset;
			Records.Add(T.Create(data, db));
			offset += T.Size;
		}
	}
}
