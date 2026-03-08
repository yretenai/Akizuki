// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using System.Runtime.InteropServices;
using Waterfall.Hash.Algorithms;

namespace Akizuki.Moo;

public abstract class BigWorldFile : IDisposable, IAsyncDisposable {
	protected BigWorldFile(Stream stream, bool validateChecksum = false) {
		BaseStream = stream;

		BigWorldHeader header = new();
		stream.ReadExactly(MemoryMarshal.AsBytes(new Span<BigWorldHeader>(ref header)));
		MooHeader = header;

		if (!validateChecksum) {
			return;
		}

		var size = (int) (stream.Length - 0x10);
		var buffer = ArrayPool<byte>.Shared.Rent(size);
		try {
			stream.ReadExactly(buffer);
			var hash = MurmurHash3Algorithm.Hash32_32(buffer);
			if (hash != header.FileChecksum) {
				throw new InvalidDataException("invalid file checksum");
			}

			stream.Seek(0x10, SeekOrigin.Begin);
		} finally {
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}

	public Stream BaseStream { get; }
	public BigWorldHeader MooHeader { get; }

	public async ValueTask DisposeAsync() {
		await DisposeAsyncCore();
		GC.SuppressFinalize(this);
	}

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing) {
		if (disposing) {
			BaseStream.Dispose();
		}
	}

	protected virtual async ValueTask DisposeAsyncCore() => await BaseStream.DisposeAsync();
}
