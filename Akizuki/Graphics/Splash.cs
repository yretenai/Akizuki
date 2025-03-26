// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Graphics;
using DragonLib.IO;

namespace Akizuki.Graphics;

public class Splash : Dictionary<string, BoundingBox> {
	public Splash(IMemoryBuffer<byte> buffer, bool leaveOpen = false) {
		using var deferred = new DeferredDisposable(() => {
			if (!leaveOpen) {
				buffer.Dispose();
			}
		});

		var reader = new MemoryReader(buffer);
		var count = reader.Read<int>();

		for (var index = 0; index < count; ++index) {
			var size = reader.Read<int>();
			var name = reader.ReadString(size);
			var bbox = reader.Read<BoundingBox>();
			this[name] = bbox;
		}
	}
}
