// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct BWDBHeader {
	public BWDBDictionary Strings { get; set; }
	public long StringsSize { get; set; }
	public long StringsPtr { get; set; }
	public BWDBDictionary ResourcePrototypes { get; set; }
	public long	PathsCount { get; set; }
	public long PathsPtr { get; set; }
	public long DatabaseCount { get; set; }
	public long DatabasePtr { get; set; }
}
