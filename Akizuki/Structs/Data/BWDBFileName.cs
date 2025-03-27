// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct BWDBFileName {
	public ulong Id { get; set; }
	public ulong ParentId { get; set; }
	public long NameLength { get; set; }
	public long NamePtr { get; set; }

	public bool Equals(BWDBFileName? other) => other?.Id == Id;
}
