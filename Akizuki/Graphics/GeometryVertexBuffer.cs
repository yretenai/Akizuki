// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Akizuki.Graphics.MeshCompression;
using Akizuki.Structs.Graphics;
using Akizuki.Structs.Graphics.VertexFormat;
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

	public IMemoryBuffer<T> DecodeBuffer<T>() where T : struct, IStandardVertex {
		if (Unsafe.SizeOf<T>() != Stride) {
			throw new InvalidOperationException("Vertex size mismatched");
		}

		return new CastMemoryBuffer<T, byte>(Buffer, true);
	}

	public MethodInfo CreateVertexGenetic(MethodInfo method) =>
		FormatName switch {
			"set3/xyznuvpc" => method.MakeGenericMethod(typeof(VertexFormatXYZNUV)),
			"set3/xyznuv2iiiwwtbpc" => method.MakeGenericMethod(typeof(VertexFormatXYZNUV2IIIWWTB)),
			"set3/xyznuv2tbpc" => method.MakeGenericMethod(typeof(VertexFormatXYZNUV2TB)),
			"set3/xyznuv2tbipc" => method.MakeGenericMethod(typeof(VertexFormatXYZNUV2TBI)),
			"set3/xyznuviiiwwpc" => method.MakeGenericMethod(typeof(VertexFormatXYZNUVIIIWW)),
			"set3/xyznuviiiwwr" => method.MakeGenericMethod(typeof(VertexFormatXYZNUVIIIWWR)),
			"set3/xyznuviiiwwtbpc" => method.MakeGenericMethod(typeof(VertexFormatXYZNUVIIIWWTB)),
			"set3/xyznuvrpc" => method.MakeGenericMethod(typeof(VertexFormatXYZNUVR)),
			"set3/xyznuvtbpc" => method.MakeGenericMethod(typeof(VertexFormatXYZNUVTB)),
			"set3/xyznuvtbipc" => method.MakeGenericMethod(typeof(VertexFormatXYZNUVTBI)),
			"set3/xyznuvtboi" => method.MakeGenericMethod(typeof(VertexFormatXYZNUVTBOI)),
			_ => throw new NotSupportedException($"Format {FormatName} is not supported"),
		};
}
