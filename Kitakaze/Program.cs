// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.PackageFileSystem.V2;
using DragonLib;

namespace Kitakaze;

public static class Program {
	public static void Main(string[] args) {
		Helpers.ResetCulture();
		foreach (var idx in Directory.EnumerateFiles($"{args[0]}/bin/11965230/idx/", "*.idx", SearchOption.TopDirectoryOnly)) {
			using var stream = new FileStream(idx, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using var pkg = new PackageV2<long>(args[0], stream, true);
		}
	}
}
