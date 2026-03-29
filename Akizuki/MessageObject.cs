// SPDX-FileCopyrightText: 2026 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using System.Text;
using Pluto.IO.Binary;

namespace Akizuki;

public class MessageObject : Dictionary<string, string> {
	public MessageObject() { }

	public MessageObject(BufferBinaryReader stream) {
		var header = stream.Read<Header>();

		if (header.Magic != Header.MO_MAGIC) {
			throw new InvalidDataException("Invalid MO Magic");
		}

		stream.Position = header.OriginalStringOffset;
		using var originalStringTable = stream.Read<StringEntry>(header.StringCount);
		stream.Position = header.TranslationStringOffset;
		using var translatedStringTable = stream.Read<StringEntry>(header.StringCount);


		for (var index = 0; index < header.StringCount; ++index) {
			var original = originalStringTable[index];
			var translated = translatedStringTable[index];

			var key = string.Empty;
			if (original.Length > 0) {
				stream.Position = original.Offset;
				key = stream.ReadCString<byte>(Encoding.UTF8, original.Length, true).ReplaceLineEndings("\n").Replace("\u00A0", " ", StringComparison.Ordinal);
			}

			var value = string.Empty;
			if (translated.Length > 0) {
				stream.Position = translated.Offset;
				value = stream.ReadCString<byte>(Encoding.UTF8, translated.Length, true).ReplaceLineEndings("\n").Replace("\u00A0", " ", StringComparison.Ordinal);
			}

			this[key] = value;
		}
	}

	public string GetTranslation(params string[] keys) {
		if (keys.Length == 0) {
			throw new ArgumentOutOfRangeException(nameof(keys));
		}

		foreach (var key in keys) {
			if (TryGetValue("IDS_" + key.ToUpperInvariant(), out var value)) {
				return value;
			}
		}

		return keys[0];
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public record struct Header {
		public const uint MO_MAGIC = 0x950412DE;

		public uint Magic { get; set; }
		public uint Revision { get; set; }
		public int StringCount { get; set; }
		public int OriginalStringOffset { get; set; }
		public int TranslationStringOffset { get; set; }
		public int HashTableSize;
		public int HashTableOffset { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public record struct StringEntry {
		public int Length { get; set; }
		public int Offset { get; set; }
	}
}
