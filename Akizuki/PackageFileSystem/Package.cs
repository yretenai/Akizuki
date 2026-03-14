// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Akizuki.Moo;
using DragonLib.IO.Binary;
using Waterfall.Compression;
using Waterfall.Hash;
using Waterfall.Hash.Basis;

namespace Akizuki.PackageFileSystem;

public abstract class Package : BigWorldFile {
	protected Package(BufferBinaryReader reader, bool validateChecksum = false) : base(reader, validateChecksum) { }
	public abstract IEnumerable<ResourceId> PresentResources { get; }

	public abstract RentedArray<byte>? OpenResource(ResourceId resource);

	protected static RentedArray<byte> OpenStreamedResource<TPointer, TResource, TTileStream>(ResourceId resource, TResource resourceHeader, Stream stream, bool validate) where TPointer : INumber<TPointer> where TResource : IPackageStreamedResource<TPointer> where TTileStream : struct, IPackageTileStream<TPointer> {
		var data = new RentedArray<byte>(int.CreateChecked(resourceHeader.Size));
		var dataSpan = data.Span;
		try {
			if (resourceHeader.CompressionType == PackageCompressionType.None || resourceHeader.CompressionSystem == PackageCompressionSystem.None) {
				Debug.Assert(TPointer.CreateChecked(resourceHeader.CompressedSize) == resourceHeader.Size);
				stream.ReadExactly(dataSpan);
			} else {
				using var compressed = new RentedArray<byte>(resourceHeader.CompressedSize);
				var dataMemory = data.Memory;
				var compressedMemory = compressed.Memory;
				var compressedSpan = compressed.Span;
				stream.ReadExactly(compressedSpan);

				switch (resourceHeader.CompressionType) {
					case PackageCompressionType.None: throw new UnreachableException();
					case PackageCompressionType.Deflate when resourceHeader.CompressionSystem is PackageCompressionSystem.Block: {
						int n;
						if (compressedSpan.Length >= 8 && MemoryMarshal.Read<uint>(compressedSpan) == 0) {
							var blockInfoCount = MemoryMarshal.Read<int>(compressedSpan[..4]);
							var blockBytes = blockInfoCount << 2;
							var blockInfos = MemoryMarshal.Cast<byte, PackageBlock>(compressedSpan[8..blockBytes]);
							var remainingSize = data.Length;
							var totalSize = remainingSize;
							var offset = 8 + blockBytes;
							const int BLOCK_SIZE = 0x10000;
							foreach (var blockInfo in blockInfos) {
								offset += blockInfo.Size;
								var start = int.CreateChecked(totalSize - remainingSize);
								var size = Math.Min(remainingSize, BLOCK_SIZE);
								var end = int.CreateChecked(start + Math.Min(remainingSize, BLOCK_SIZE));
								Debug.Assert(blockInfo.IsCompressed is 0 or 1);
								n = CompressionHelper.Decompress(blockInfo.IsCompressed == 0 ? CompressionType.None : CompressionType.Deflate, compressedMemory[offset..(offset + blockInfo.Size)], dataMemory[start..end]);
								Debug.Assert(n == size);
								offset += blockInfo.Size;
								remainingSize -= size;
							}

							break;
						}

						n = CompressionHelper.Decompress(CompressionType.Deflate, compressedMemory, dataMemory);
						Debug.Assert(n == data.Length);
						break;
					}
					case >= PackageCompressionType.OodleKraken and <= PackageCompressionType.OodleHydra when resourceHeader.CompressionSystem is PackageCompressionSystem.TileStream: {
						var streamHeader = MemoryMarshal.Read<TTileStream>(compressedMemory.Span);
						var remainingSize = long.CreateChecked(streamHeader.Size);
						var totalSize = remainingSize;
						var offset = int.CreateChecked(streamHeader.DataOffset) + 8;
						var blocks = MemoryMarshal.Cast<byte, int>(compressedMemory.Span[Unsafe.SizeOf<TTileStream>()..])[..streamHeader.BlockCount];
						Debug.Assert(streamHeader.Size == resourceHeader.Size);
						foreach (var block in blocks) {
							var start = int.CreateChecked(totalSize - remainingSize);
							var size = Math.Min(remainingSize, streamHeader.BlockSize);
							var end = int.CreateChecked(start + Math.Min(remainingSize, streamHeader.BlockSize));
							var n = CompressionHelper.Decompress(CompressionType.Oodle, compressedMemory[offset..(offset + block)], dataMemory[start..end]);
							Debug.Assert(n == size);
							offset += block;
							remainingSize -= size;
						}

						break;
					}
					default: throw new NotSupportedException($"compression type {resourceHeader.CompressionType} with subsystem {resourceHeader.CompressionSystem} is not supported");
				}
			}

			if (validate) {
				var hash = CRC.HashData(CRC32Variants.ISO, dataSpan);
				if (hash != resourceHeader.Checksum) {
					throw new InvalidDataException("Checksum mismatch");
				}

				AkizukiLog.Debug("{File} Passed Checksum Validation", resource.ToDebugString());
			}
		} catch {
			data.Dispose();
			throw;
		}

		return data;
	}
}
