// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using Akizuki.Data;
using DragonLib.IO;

namespace Akizuki.Unpack.Conversion;

internal static class Assets {
	internal static void Save(ProgramFlags flags, BigWorldDatabase assets) {
		foreach (var (assetId, prototypeId) in assets.ResourceToPrototype) {
			if (assets.Resolve(prototypeId) is not { } prototype) {
				continue;
			}

			var path = Path.Combine(flags.OutputDirectory, assets.Paths.TryGetValue(assetId, out var name) ? name.TrimStart('/', '.') : $"res/assets/{assetId:x16}.{prototype.GetType().Name}");
			if (!Path.HasExtension(path)) {
				// special edge case for .xml
				path = Path.GetDirectoryName(path) + "." + Path.GetFileName(path);
			}

			path += ".json";

			AkizukiLog.Information("{Value}", name ?? $"{assetId:x16}");

			if (flags.Dry) {
				continue;
			}

			var dir = Path.GetDirectoryName(path) ?? flags.OutputDirectory;
			Directory.CreateDirectory(dir);

			using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			JsonSerializer.Serialize(stream, prototype, Program.Options);
			stream.WriteByte((byte) '\n');
		}
	}

	public static bool SaveData(string path, ProgramFlags flags, IMemoryBuffer<byte> data) {
		var pickled = new PickledData(data);
		if (flags.Dry) {
			return false;
		}

		var dir = Path.GetDirectoryName(path) ?? flags.OutputDirectory;
		Directory.CreateDirectory(dir);
		using var stream = new FileStream(Path.ChangeExtension(path, ".json"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(stream, pickled.Data, Program.SafeOptions);
		stream.WriteByte((byte) '\n');
		return false;
	}
}
