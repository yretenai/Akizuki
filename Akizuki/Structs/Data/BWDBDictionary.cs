// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct BWDBDictionary {
	public long Count { get; set; }
	public long KeyPtr { get; set; }
	public long ValuePtr { get; set; }
}
