// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Akizuki.Graphics.MeshCompression;
using Akizuki.Structs.Graphics;
using DragonLib.IO;

namespace Akizuki.Graphics;

public sealed class GeometryVertexBuffer : IDisposable {
	public GeometryVertexBuffer(GeometryVertexBufferHeader header, MemoryReader buffer) {
		Stride = header.VertexStride;
		var pos = buffer.Offset;
		buffer.Offset = (int) (pos + header.VertexFormatPtr + 8);
		FormatName = buffer.ReadString((int) (header.VertexFormatLength - 1));

		buffer.Offset = (int) (pos + header.BufferPtr);
		var partition = buffer.Partition(header.BufferLength);
		if (MemoryMarshal.Read<uint>(partition.Span) == 0x44434E45) {
			VertexCount = MemoryMarshal.Read<int>(partition.Span[4..]);
			Buffer = new MemoryBuffer<byte>(VertexCount * Stride);
			MeshOptimizerVertexDecoder.DecodeVertexBuffer(VertexCount, Stride, partition.Span[8..], Buffer.Span);
			partition.Dispose();
		} else {
			Buffer = partition;
			VertexCount = Buffer.Length / Stride;
		}
	}

	public int Stride { get; set; }
	public int VertexCount { get; set; }
	public string FormatName { get; set; }
	public IMemoryBuffer<byte> Buffer { get; set; }

	public void Dispose() => Buffer.Dispose();
}
