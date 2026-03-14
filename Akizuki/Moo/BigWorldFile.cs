// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.AssetDb;
using Akizuki.PackageFileSystem.V1;
using Akizuki.PackageFileSystem.V2;
using DragonLib.IO.Binary;
using Waterfall.Hash.Algorithms;

namespace Akizuki.Moo;

public abstract class BigWorldFile : IDisposable {
	protected BigWorldFile(BufferBinaryReader reader, bool validateChecksum = false) {
		var header = MooHeader = reader.Read<BigWorldHeader>();

		if (!validateChecksum) {
			return;
		}

		var size = reader.Length - 0x10;
		using var buffer = reader.ReadSharedBytes(size);
		var hash = MurmurHash3Algorithm.Hash32_32(buffer.Span);
		if (hash != header.FileChecksum) {
			throw new InvalidDataException("invalid file checksum");
		}

		reader.Position = 0x10;
	}

	public BigWorldHeader MooHeader { get; }

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing) { }

	public static BigWorldFile? OpenByVersion(string basePath, BufferBinaryReader reader, bool validate = false) {
		var header = reader.Peek<BigWorldHeader>();

		if (header.Endianness != BigWorldEndianness.Little) {
			AkizukiLog.Error("Cannot handle BigEndian BigWorld Moo File: {Header}", header);
			return default;
		}

		if (header.Magic == BigWorldMagic.PackageIndex) {
			switch (header.PointerSize) {
				case 64 when header.Version.Major == 2:
					return new PackageV2<long>(basePath, reader, validate);
				case 32 when header.Version.Major == 2:
					return new PackageV2<int>(basePath, reader, validate);
				case 64 when header.Version.Major == 1:
					return new PackageV1<long>(basePath, reader, validate);
				case 32 when header.Version.Major == 1:
					return new PackageV1<int>(basePath, reader, validate);
			}
		} else if (header.Magic == BigWorldMagic.AssetDatabase) {
			switch  (header.PointerSize) {
				case 64 when header.Version.Major == 1:
					return new AssetDatabase(reader, validate);
			}
		}

		AkizukiLog.Error("Cannot handle BigWorld Moo File: {Header}", header);
		return default;
	}
}
