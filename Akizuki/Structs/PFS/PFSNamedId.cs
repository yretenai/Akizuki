// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.PFS;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct PFSNamedId {
	public long NameLength { get; set; }
	public long NamePtr { get; set; }
	public ulong Id { get; set; }

	public bool Equals(PFSNamedId? other) => other?.Id == Id;
}
