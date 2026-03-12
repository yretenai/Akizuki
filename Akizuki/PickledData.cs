// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.IO.Compression;
using System.Runtime.InteropServices;
using DragonLib.IO;
using DragonLib.IO.Binary;
using Ferment;

namespace Akizuki;

public static class PickledData {
	public static PickleObject Create(RentedArray<byte> buffer, string region = "", bool leaveOpen = false) {
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

			if(pickler.Read() is not {} data) {
				throw new InvalidOperationException("failed to correctly unpickle");
			}

			if (data is GameDataObject gameParams) {
				data = gameParams.TryGetValue(region, out var regionParams) ? regionParams : gameParams[""];
			}

			if (data is object[] { Length: 2 } dataArray) {
				data = dataArray[1];
			}

			if (data is not GameDataObject valueArray) {
				throw new InvalidOperationException("failed to correctly unpickle");
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
