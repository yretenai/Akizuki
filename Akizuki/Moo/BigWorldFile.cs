// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Akizuki.PackageFileSystem.V2;
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
			stream.ReadExactly(buffer.AsSpan(0, size));
			var hash = MurmurHash3Algorithm.Hash32_32(buffer.AsSpan(0, size));
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

	public static BigWorldFile? OpenByVersion(string basePath, Stream stream, bool validate = false) {
		BigWorldHeader header = new();
		stream.ReadExactly(MemoryMarshal.AsBytes(new Span<BigWorldHeader>(ref header)));
		stream.Position -= Unsafe.SizeOf<BigWorldHeader>();

		if (header.Magic == BigWorldMagic.PackageIndex) {
			switch (header.PointerSize) {
				case 64 when header.Version.Major == 2:
					return new PackageV2<long>(basePath, stream, validate);
				case 32 when header.Version.Major == 2:
					return new PackageV2<int>(basePath, stream, validate);
			}
		}

		AkizukiLog.Error("Cannot handle BigWorld Moo File: {Header}", header);
		return null;
	}
}
