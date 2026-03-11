// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Akizuki.AssetDb;

public enum AssetTablePrototypeState : byte {
	Unloaded = 0,
	Loading = 1,
	Loaded = 2,
	Deleted = 3,
}
