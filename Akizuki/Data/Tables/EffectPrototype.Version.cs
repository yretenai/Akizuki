// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using DragonLib.Hash.Algorithms;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class EffectPrototype : IPrototype {
	static EffectPrototype() {
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

	public static IPrototype Create(MemoryReader reader) => new EffectPrototype(reader);

	public static void AppendVersion(StringBuilder sb) { }

	public static uint Version { get; }
	public static uint Id { get; } = MurmurHash3Algorithm.Hash32_32("EffectPrototype"u8);
	public static string PrototypeName => "EffectPrototype";
	public static int Size => 0; // Unsafe.SizeOf<EffectPrototypeHeader>();
}
