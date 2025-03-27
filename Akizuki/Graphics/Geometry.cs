// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using Akizuki.Structs.Data;
using Akizuki.Structs.Graphics;
using DragonLib.IO;

namespace Akizuki.Graphics;

public sealed class Geometry : IDisposable {
	public Geometry(IMemoryBuffer<byte> buffer) {
		using var reader = new MemoryReader(buffer, true);
		var pos = reader.Offset;

		var header = reader.Read<GeometryHeader>();

		reader.Offset = (int) (pos + header.VertexNamesPtr);
		foreach (var name in reader.Read<GeometryName>(header.VertexNameCount)) {
			SharedVertexBuffers[name.Name] = name;
		}

		reader.Offset = (int) (pos + header.IndexNamesPtr);
		foreach (var name in reader.Read<GeometryName>(header.IndexNameCount)) {
			SharedIndexBuffers[name.Name] = name;
		}

		var vboStart = reader.Offset = (int) (pos + header.VertexBuffersPtr);
		var vbos = reader.Read<GeometryVertexBufferHeader>(header.VertexBufferCount);
		var oneVbo = Unsafe.SizeOf<GeometryVertexBufferHeader>();
		for (var index = 0; index < vbos.Length; index++) {
			reader.Offset = vboStart + index * oneVbo;
			MergedVertexBuffers.Add(new GeometryVertexBuffer(vbos[index], reader));
		}

		var iboStart = reader.Offset = (int) (pos + header.IndexBuffersPtr);
		var ibos = reader.Read<GeometryIndexBufferHeader>(header.IndexBufferCount);
		var oneIbo = Unsafe.SizeOf<GeometryIndexBufferHeader>();
		for (var index = 0; index < ibos.Length; index++) {
			reader.Offset = iboStart + index * oneIbo;
			MergedIndexBuffers.Add(new GeometryIndexBuffer(ibos[index], reader));
		}

		var armorStart = reader.Offset = (int) (pos + header.ArmorBuffersPtr);
		var armors = reader.Read<GeometrySimpleBufferHeader>(header.ArmorBufferCount);
		var oneArmor = Unsafe.SizeOf<GeometrySimpleBufferHeader>();
		for (var index = 0; index < armors.Length; index++) {
			reader.Offset = armorStart + index * oneArmor;
			Armors.Add(new GeometryArmor(armors[index], reader));
		}
	}

	public Dictionary<StringId, GeometryName> SharedVertexBuffers { get; set; } = [];
	public Dictionary<StringId, GeometryName> SharedIndexBuffers { get; set; } = [];
	public List<GeometryVertexBuffer> MergedVertexBuffers { get; set; } = [];
	public List<GeometryIndexBuffer> MergedIndexBuffers { get; set; } = [];
	public List<GeometryArmor> Armors { get; set; } = [];

	public void Dispose() {
		foreach (var buffer in MergedVertexBuffers) {
			buffer.Dispose();
		}

		foreach (var buffer in MergedIndexBuffers) {
			buffer.Dispose();
		}

		foreach (var buffer in Armors) {
			buffer.Dispose();
		}
	}
}
