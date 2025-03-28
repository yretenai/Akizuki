// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using Akizuki.Data;

namespace Akizuki.Conversion;

public static class Assets {
	public static void Save(string outputDirectory, IConversionOptions flags, Func<string?, bool> check, BigWorldDatabase assets) {
		foreach (var (assetId, prototypeId) in assets.ResourceToPrototype) {
			if (assets.Resolve(prototypeId) is not { } prototype) {
				continue;
			}

			var path = Path.Combine(outputDirectory, assets.Paths.TryGetValue(assetId, out var name) ? name.TrimStart('/', '.') : $"res/assets/{assetId:x16}.{prototype.GetType().Name}");
			if (!check(name)) {
				continue;
			}

			if (!Path.HasExtension(path)) {
				// special edge case for .xml
				path = Path.GetDirectoryName(path) + "." + Path.GetFileName(path);
			}

			path += ".json";

			AkizukiLog.Information("{Value}", name ?? $"{assetId:x16}");

			if (flags.Dry) {
				continue;
			}

			var dir = Path.GetDirectoryName(path) ?? outputDirectory;
			Directory.CreateDirectory(dir);

			using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			JsonSerializer.Serialize(stream, prototype, JsonOptions.Options);
			stream.WriteByte((byte) '\n');
		}
	}

	public static bool SaveData(string path, IConversionOptions flags, Func<string?, bool> check, PickledData pickled) {
		if (flags.Dry) {
			return false;
		}

		path = Path.ChangeExtension(path, null);
		Directory.CreateDirectory(path);
		foreach (var (key, data) in pickled.Values) {
			var name = $"res/content/GameParams/{key}.json";
			if (!check(name)) {
				continue;
			}

			AkizukiLog.Information("{Path}", name);
			var paramPath = Path.Combine(path, key + ".json");
			using var stream = new FileStream(paramPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			JsonSerializer.Serialize(stream, data, JsonOptions.SafeOptions);
			stream.WriteByte((byte) '\n');
		}

		return false;
	}
}
