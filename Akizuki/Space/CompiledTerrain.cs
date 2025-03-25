// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Space;
using DragonLib.IO;
using Silk.NET.Maths;

namespace Akizuki.Space;

public sealed class CompiledTerrain : IDisposable {
	public CompiledTerrain(IMemoryBuffer<byte> buffer, bool leaveOpen = false) {
		using var deferred = new DeferredDisposable(() => {
			if (!leaveOpen) {
				buffer.Dispose();
			}
		});

		var data = new SpanReader(buffer.Span);

		Header = data.Read<CompiledTerrainHeader>();
		if (Header.Magic != CompiledTerrainHeader.TRBMagic) {
			throw new InvalidDataException("File is not recognised as a Compiled Terrain File");
		}

		// min, max for each chunk
		var chunks = new MemoryBuffer<Vector2D<float>>(Header.Chunks * Header.Chunks);
		ChunkRanges = chunks;
		data.Read(chunks.Span);

		var terrain = new MemoryBuffer<float>(Header.Width * Header.Height);
		Data = terrain;
		var terrainSpan = terrain.Span;

		// instead of repeating float values
		// repeat the same value twice then state how many times that value needs to be repeated.
		for (var i = 0; i < terrain.Length;) {
			var point = data.Read<float>();
			var nextPoint = data.Read<float>();
			while (Math.Abs(point - nextPoint) > float.Epsilon) {
				terrainSpan[i++] = point;
				(point, nextPoint) = (nextPoint, data.Read<float>());
			}

			var repeat = data.Read<int>();
			terrainSpan.Slice(i, repeat).Fill(nextPoint);
			i += repeat;
		}
	}

	public CompiledTerrainHeader Header { get; }
	public IMemoryBuffer<Vector2D<float>> ChunkRanges { get; }
	public IMemoryBuffer<float> Data { get; }

	public void Dispose() {
		ChunkRanges.Dispose();
		Data.Dispose();
	}
}
