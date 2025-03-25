// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data.Tables;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct ModelPrototypeHeader {
	public ulong VisualResourceId { get; set; }
	public ModelMiscType MiscType { get; set; }
	public byte AnimationsCount { get; set; }
	public byte DyesCount { get; set; }
	public long AnimationsPtr { get; set; }
	public long DyesPtr { get; set; }
}
