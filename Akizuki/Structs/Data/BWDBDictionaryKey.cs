// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct BWDBDictionaryKey<T> where T : unmanaged {
	public T Key { get; set; }
	public T BucketId { get; set; }
}
