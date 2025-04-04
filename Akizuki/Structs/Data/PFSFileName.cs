// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct PFSFileName {
	public PFSNamedId Name { get; set; }
	public ulong ParentId { get; set; }

	public bool Equals(PFSFileName? other) => other?.Name.Id == Name.Id;
}
