// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Data;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class EffectPresetPrototype {
	public EffectPresetPrototype(MemoryReader data) {
		HighPreset = data.Read<ResourceId>();
		LowPreset = data.Read<ResourceId>();
	}

	public ResourceId HighPreset { get; set; }
	public ResourceId LowPreset { get; set; }
}
