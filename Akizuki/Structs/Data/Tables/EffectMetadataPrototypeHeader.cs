// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data.Tables;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct EffectMetadataPrototypeHeader {
	public int NameLength { get; set; }
	public long NamePtr { get; set; }
	public EffectMetaPrototype Metadata { get; set; }
}
