// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Akizuki.Graphics.MeshCompression;
using Akizuki.Structs.Graphics;
using DragonLib.IO;

namespace Akizuki.Graphics;

public sealed class GeometryIndexBuffer : IDisposable {
	public GeometryIndexBuffer(GeometryIndexBufferHeader header, MemoryReader buffer) {
		Stride = header.IndexStride;
		var pos = buffer.Offset;

		buffer.Offset = (int) (pos + header.BufferPtr);
		var partition = buffer.Partition(header.BufferLength);
		if (MemoryMarshal.Read<uint>(partition.Span) == 0x44434E45) {
			IndexCount = MemoryMarshal.Read<int>(partition.Span[4..]);
			Buffer = new MemoryBuffer<byte>(IndexCount * Stride);
			MeshOptimizerIndexDecoder.DecodeIndexBuffer(IndexCount, Stride, partition.Span[8..], Buffer.Span);
			partition.Dispose();
		} else {
			Buffer = partition;
			IndexCount = Buffer.Length / Stride;
		}
	}

	public int Stride { get; set; }
	public int IndexCount { get; set; }
	public IMemoryBuffer<byte> Buffer { get; set; }

	public void Dispose() => Buffer.Dispose();
}
