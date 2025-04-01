// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Text.Json;
using Akizuki.Conversion.Utility;
using Akizuki.Data;
using Akizuki.Structs.Data.Camouflage;

namespace Akizuki.Conversion;

public static class AssetConverter {
	[MethodImpl(MethodConstants.Optimize)]
	public static void SavePaths(string path, IConversionOptions flags, PackageFileSystem list) {
		if (flags.Dry) {
			return;
		}

		using var stream = new FileStream(Path.ChangeExtension(path, ".json"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(stream, list, JsonOptions.Options);
		stream.WriteByte((byte) '\n');
	}

	[MethodImpl(MethodConstants.Optimize)]
	public static void SavePaths(string path, IConversionOptions flags, BigWorldDatabase list) {
		if (flags.Dry) {
			return;
		}

		using var stream = new FileStream(Path.ChangeExtension(path, ".json"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(stream, list, JsonOptions.Options);
		stream.WriteByte((byte) '\n');
	}

	[MethodImpl(MethodConstants.Optimize)]
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

	[MethodImpl(MethodConstants.Optimize)]
	public static void SaveLocale(string outputDirectory, IConversionOptions flags, string locale, MessageObject text) {
		var name = $"res/texts/{locale}.json";
		var path = Path.Combine(outputDirectory, name);

		AkizukiLog.Information("{Value}", name);

		if (flags.Dry) {
			return;
		}

		var dir = Path.GetDirectoryName(path) ?? outputDirectory;
		Directory.CreateDirectory(dir);

		using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(stream, text, JsonOptions.Options);
		stream.WriteByte((byte) '\n');
	}

	[MethodImpl(MethodConstants.Optimize)]
	public static void SaveCamouflages(string outputDirectory, IConversionOptions flags, CamouflageRoot camouflages) {
		const string Name = "res/camouflages.json";
		var path = Path.Combine(outputDirectory, Name);

		AkizukiLog.Information("{Value}", Name);

		if (flags.Dry) {
			return;
		}

		var dir = Path.GetDirectoryName(path) ?? outputDirectory;
		Directory.CreateDirectory(dir);

		using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(stream, camouflages, JsonOptions.FromXmlOptions);
		stream.WriteByte((byte) '\n');
	}

	[MethodImpl(MethodConstants.Optimize)]
	public static bool SaveData(string path, IConversionOptions flags, Func<string?, bool> check, PickleObject pickled) {
		if (flags.Dry) {
			return false;
		}

		path = Path.ChangeExtension(path, null);
		Directory.CreateDirectory(path);
		foreach (var (key, data) in pickled) {
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
