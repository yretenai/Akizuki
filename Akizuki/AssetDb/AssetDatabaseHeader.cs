// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.AssetDb;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct AssetDatabaseHeader {
	public AssetDatabaseMap StringTable { get; set; }
	public AssetDatabaseArray StringList { get; set; }
	public AssetDatabaseMap PrototypeTable { get; set; }
	public AssetDatabaseArray PathList { get; set; }
	public AssetDatabaseArray PrototypeList { get; set; }
}
