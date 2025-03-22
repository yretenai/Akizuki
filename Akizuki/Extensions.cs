// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;

namespace Akizuki;

public static class Extensions {
	public static byte[] ToRented(this Stream stream, out int size, ArrayPool<byte>? pool = null) {
		size = (int) (stream.Length - stream.Position);
		var rent = (pool ?? ArrayPool<byte>.Shared).Rent(size);
		stream.ReadExactly(rent.AsSpan(0, size));
		return rent;
	}
}
