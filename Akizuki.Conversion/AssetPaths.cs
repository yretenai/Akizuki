// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using Akizuki.Data;

namespace Akizuki.Conversion;

public static class AssetPaths {
	public static void Save(string path, IConversionOptions flags, PackageFileSystem list) {
		if (flags.Dry) {
			return;
		}

		using var stream = new FileStream(Path.ChangeExtension(path, ".json"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(stream, list, JsonOptions.Options);
		stream.WriteByte((byte) '\n');
	}

	public static void Save(string path, IConversionOptions flags, BigWorldDatabase list) {
		if (flags.Dry) {
			return;
		}

		using var stream = new FileStream(Path.ChangeExtension(path, ".json"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(stream, list, JsonOptions.Options);
		stream.WriteByte((byte) '\n');
	}
}
