// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Akizuki.Moo;

namespace Akizuki.AssetDb;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct AssetDatabaseName {
	public ResourceId Id { get; set; }
	public ResourceId ParentId { get; set; }
	public AssetDatabaseArray Pointer { get; set; }
}
