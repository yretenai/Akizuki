// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using Akizuki.Data;

namespace Akizuki.Unpack.Conversion;

internal static class AssetPaths {
	internal static void Save(string path, ProgramFlags flags, PackageFileSystem list) {
		if (flags.Dry) {
			return;
		}

		using var stream = new FileStream(Path.ChangeExtension(path, ".json"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(stream, list, Program.Options);
		stream.WriteByte((byte) '\n');
	}

	internal static void Save(string path, ProgramFlags flags, BigWorldDatabase list) {
		if (flags.Dry) {
			return;
		}

		using var stream = new FileStream(Path.ChangeExtension(path, ".json"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(stream, list, Program.Options);
		stream.WriteByte((byte) '\n');
	}
}
