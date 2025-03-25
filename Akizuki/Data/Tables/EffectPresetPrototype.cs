// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class EffectPresetPrototype {
	public EffectPresetPrototype(MemoryReader data, BigWorldDatabase db) {
		HighPreset = db.GetPath(data.Read<ulong>());
		LowPreset = db.GetPath(data.Read<ulong>());
	}

	public string HighPreset { get; set; }
	public string LowPreset { get; set; }
}
