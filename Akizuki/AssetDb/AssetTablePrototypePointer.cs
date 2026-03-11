// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using DragonLib.SourceGen.BitStructGenerator;

namespace Akizuki.AssetDb;

[BitStruct(4)]
public partial struct AssetTablePrototypePointer {
	[BitField(2)]
	public partial AssetTablePrototypeState State { get; set; }

	[BitField(6)]
	public partial uint TableIndex { get; set; }

	[BitField(24)]
	public partial uint RecordIndex { get; set; }
}
