// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Akizuki.Moo;

namespace Akizuki.AssetDb;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct AssetDatabaseTableHeader {
	public StringId Id { get; set; }
	public uint Version { get; set; }
	public AssetDatabaseArray Prototypes { get; set; }
}
