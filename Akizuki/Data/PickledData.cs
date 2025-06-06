// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.IO.Compression;
using System.Runtime.InteropServices;
using DragonLib.IO;
using Ferment;

namespace Akizuki.Data;

public static class PickledData {
	public static PickleObject Create(IMemoryBuffer<byte> buffer, bool leaveOpen = false) {
		using var deferred = new DeferredDisposable(() => {
			if (!leaveOpen) {
				buffer.Dispose();
			}
		});

		if (MemoryMarshal.Read<uint>(buffer.Span) != 0x6E696225) {
			throw new InvalidDataException("Not a %bin file");
		}

		var compressed = buffer.Memory[4..];
		compressed.Span.Reverse();

		var result = new PickleObject();
		using var pinned = compressed.Pin();
		unsafe {
			using var stream = new UnmanagedMemoryStream((byte*) pinned.Pointer, compressed.Length);
			using var decompressor = new ZLibStream(stream, CompressionMode.Decompress);
			using var pickler = new Unpickler(decompressor);

			var data = pickler.Read()!;
			if (data is not object[] dataArray || dataArray.Length < 2) {
				throw new InvalidOperationException();
			}

			if (dataArray[1] is not GameDataObject valueArray) {
				throw new InvalidOperationException();
			}

			foreach (var (key, value) in valueArray) {
				if (value is not GameDataObject param) {
					throw new InvalidOperationException();
				}

				result[key.ToString()!] = param;
			}
		}

		return result;
	}
}
