using System.IO.Compression;
using System.Runtime.InteropServices;
using DragonLib.IO;
using Ferment;

namespace Akizuki.Data;

public class PickledData {
	public PickledData(IMemoryBuffer<byte> buffer, bool leaveOpen = false) {
		using var deferred = new DeferredDisposable(() => {
			if (!leaveOpen) {
				buffer.Dispose();
			}
		});

		if (MemoryMarshal.Read<uint>(buffer.Span) != 0x6E696225) {
			throw new InvalidDataException("Not a %bin file");
		}

		var compressed = buffer.Memory[4..];
		compressed.Span.Reverse();

		using var pinned = compressed.Pin();
		unsafe {
			using var stream = new UnmanagedMemoryStream((byte*) pinned.Pointer, compressed.Length);
			using var decompressor = new ZLibStream(stream, CompressionMode.Decompress);
			using var pickler = new Unpickler(decompressor);
			Data = pickler.Read()!;
		}
	}

	public object Data { get; set; }
}
