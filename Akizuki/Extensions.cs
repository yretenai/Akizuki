// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using DragonLib.IO;

namespace Akizuki;

public static class Extensions {
	public static IMemoryBuffer ToRented(this Stream stream) {
		var size = (int) (stream.Length - stream.Position);
		var memory = new MemoryBuffer(size);
		stream.ReadExactly(memory.Span);
		return memory;
	}
}
