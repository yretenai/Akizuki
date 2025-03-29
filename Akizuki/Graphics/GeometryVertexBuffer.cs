// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Akizuki.Graphics.MeshCompression;
using Akizuki.Structs.Graphics;
using Akizuki.Structs.Graphics.VertexFormat;
using DragonLib.IO;

namespace Akizuki.Graphics;

public sealed class GeometryVertexBuffer : IDisposable {
	public GeometryVertexBuffer(GeometryVertexBufferHeader header, MemoryReader buffer) {
		Header = header;
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
	public GeometryVertexBufferHeader Header { get; set; }
	public IMemoryBuffer<byte> Buffer { get; set; }

	public VertexInfo Info =>
		FormatName switch {
			"set3/xyznuvpc" => VertexFormatXYZNUV.VertexInfo,
			"set3/xyznuv2iiiwwtbpc" => VertexFormatXYZNUV2IIIWWTB.VertexInfo,
			"set3/xyznuv2tbpc" => VertexFormatXYZNUV2TB.VertexInfo,
			"set3/xyznuv2tbipc" => VertexFormatXYZNUV2TBI.VertexInfo,
			"set3/xyznuviiiwwpc" => VertexFormatXYZNUVIIIWW.VertexInfo,
			"set3/xyznuviiiwwr" => VertexFormatXYZNUVIIIWWR.VertexInfo,
			"set3/xyznuviiiwwtbpc" => VertexFormatXYZNUVIIIWWTB.VertexInfo,
			"set3/xyznuvrpc" => VertexFormatXYZNUVR.VertexInfo,
			"set3/xyznuvtbpc" => VertexFormatXYZNUVTB.VertexInfo,
			"set3/xyznuvtbipc" => VertexFormatXYZNUVTBI.VertexInfo,
			"set3/xyznuvtboi" => VertexFormatXYZNUVTBOI.VertexInfo,
			_ => throw new NotSupportedException($"Format {FormatName} is not supported"),
		};

	public void Dispose() => Buffer.Dispose();

	public IMemoryBuffer<T> DecodeBuffer<T>() where T : struct, IStandardVertex {
		if (Unsafe.SizeOf<T>() != Stride) {
			throw new InvalidOperationException("Vertex size mismatched");
		}

		return new CastMemoryBuffer<T, byte>(Buffer, true);
	}
}
