// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public record struct MOHeader {
	public const uint MO_MAGIC = 0x950412DE;

	public uint Magic { get; set; }
	public uint Revision { get; set; }
	public int StringCount { get; set; }
	public int OriginalStringOffset { get; set; }
	public int TranslationStringOffset { get; set; }
	public int HashTableSize;
	public int HashTableOffset { get; set; }
}
