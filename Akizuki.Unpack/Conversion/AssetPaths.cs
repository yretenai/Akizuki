// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using Akizuki.Data;

namespace Akizuki.Unpack.Conversion;

internal static class AssetPaths {
	internal static void Save(string path, PackageFileSystem list) {
		using var stream = new FileStream(Path.ChangeExtension(path, ".json"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(stream, list, Program.Options);
	}

	internal static void Save(string path, BigWorldDatabase list) {
		using var stream = new FileStream(Path.ChangeExtension(path, ".json"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(stream, list, Program.Options);
	}
}
