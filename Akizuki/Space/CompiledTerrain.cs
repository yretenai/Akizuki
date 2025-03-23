// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Space;
using DragonLib.IO;

namespace Akizuki.Space;

public sealed class CompiledTerrain : IDisposable {
	public CompiledTerrain(MemoryBuffer<byte> buffer) {
		var data = new SpanReader(buffer.Span);

		Header = data.Read<CompiledTerrainHeader>();
		if (Header.Magic != CompiledTerrainHeader.TRBMagic) {
			throw new InvalidDataException("File is not recognised as a Compiled Terrain File");
		}

		var sd = new MemoryBuffer<float>((Header.SDSize * Header.SDSize) << 1);
		SDTerrain = sd;
		data.Read(sd.Span);

		var hd = new MemoryBuffer<float>(Header.Width * Header.Height);
		HDTerrain = hd;

		var hdSpan = hd.Span;
		for (var i = 0; i < hd.Length;) {
			var point = data.Read<float>();
			var nextPoint = data.Read<float>();
			while (Math.Abs(point - nextPoint) > float.Epsilon) {
				hdSpan[i++] = point;
				point = nextPoint;
				nextPoint = data.Read<float>();
			}

			var repeat = data.Read<int>();
			hdSpan.Slice(i, repeat).Fill(nextPoint);
			i += repeat;
		}
	}

	public CompiledTerrainHeader Header { get; }
	public IMemoryBuffer<float> SDTerrain { get; }
	public IMemoryBuffer<float> HDTerrain { get; }

	public void Dispose() {
		SDTerrain.Dispose();
		HDTerrain.Dispose();
	}
}
