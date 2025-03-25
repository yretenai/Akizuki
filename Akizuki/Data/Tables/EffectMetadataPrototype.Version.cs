// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using DragonLib.Hash.Algorithms;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class EffectMetadataPrototype : IPrototype {
	static EffectMetadataPrototype() {
		var sb = new StringBuilder(0x4000);
		sb.Append(PrototypeName);
		AppendVersion(sb);

		Span<byte> version = stackalloc byte[0x4000];
		var offset = 0;
		foreach (var chunk in sb.GetChunks()) {
			offset += Encoding.ASCII.GetBytes(chunk.Span, version[offset..]);
		}

		Version = MurmurHash3Algorithm.Hash32_32(version[..offset]);
	}

	public static IPrototype Create(MemoryReader reader, BigWorldDatabase db) => new EffectMetadataPrototype(reader, db);

	public static void AppendVersion(StringBuilder sb) {
	}

	public static uint Version { get; }
	public static uint Id { get; } = MurmurHash3Algorithm.Hash32_32("EffectMetadataPrototype"u8);
	public static string PrototypeName => "EffectMetadataPrototype";
	public static int Size => 0; // Unsafe.SizeOf<EffectMetadataPrototypeHeader>();
}
