// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DragonLib.IO.Binary;

namespace Akizuki.AssetDb;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct AssetDatabaseArray {
	public static int Size { get; } = Unsafe.SizeOf<AssetDatabaseArray>();

	public long Length { get; set; }
	public long Offset { get; set; }

	public RentedArray<T> Read<T>(long structOffset, BufferBinaryReader reader) where T : struct {
		reader.Position = (int) (structOffset + Offset);
		return reader.Read<T>((int) Length);
	}
}
