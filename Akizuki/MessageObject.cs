// SPDX-FileCopyrightText: 2026 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace Akizuki;

public class MessageObject : Dictionary<string, string> {
	public MessageObject() { }

	public MessageObject(Stream stream) {
		var header = new Header();
		stream.ReadExactly(MemoryMarshal.AsBytes(new Span<Header>(ref header)));

		if (header.Magic != Header.MO_MAGIC) {
			throw new InvalidDataException("Invalid MO Magic");
		}

		var originalStringTable = ArrayPool<StringEntry>.Shared.Rent(header.StringCount);
		var translatedStringTable = ArrayPool<StringEntry>.Shared.Rent(header.StringCount);
		var slop = ArrayPool<byte>.Shared.Rent(0x100000);
		try {
			stream.Position = header.OriginalStringOffset;
			stream.ReadExactly(MemoryMarshal.AsBytes(originalStringTable.AsSpan(0, header.StringCount)));
			stream.Position = header.TranslationStringOffset;
			stream.ReadExactly(MemoryMarshal.AsBytes(translatedStringTable.AsSpan(0, header.StringCount)));


			for (var index = 0; index < header.StringCount; ++index) {
				var key = string.Empty;
				var value = string.Empty;
				var original = originalStringTable[index];
				var translated = translatedStringTable[index];

				if (original.Length > 0) {
					stream.Position = original.Offset;
					var tmp = slop.AsSpan(0, original.Length);
					stream.ReadExactly(tmp);
					key = Encoding.UTF8.GetString(tmp).ReplaceLineEndings("\n")
								  .Replace("\u00A0", " ", StringComparison.Ordinal);
				}

				if (translated.Length > 0) {
					stream.Position = translated.Offset;
					var tmp = slop.AsSpan(0, translated.Length);
					stream.ReadExactly(tmp);
					value = Encoding.UTF8.GetString(tmp).ReplaceLineEndings("\n")
									.Replace("\u00A0", " ", StringComparison.Ordinal);
				}

				this[key] = value;
			}
		} finally {
			ArrayPool<StringEntry>.Shared.Return(originalStringTable);
			ArrayPool<StringEntry>.Shared.Return(translatedStringTable);
			ArrayPool<byte>.Shared.Return(slop);
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
