// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Data;
using Akizuki.Data.Tables;
using Akizuki.Structs.Data;
using DragonLib.IO;
using Serilog;

namespace Akizuki;

public sealed class ResourceManager : IDisposable {
	public ResourceManager(BigWorldDatabase db) {
		if (Instance != null) {
			throw new InvalidOperationException("Only one instance of ResourceManager is allowed");
		}

		Instance = this;
		Database = db;
	}

	public ResourceManager(string idxDir, string pkgDir, bool validate = false) {
		if (Instance != null) {
			throw new InvalidOperationException("Only one instance of ResourceManager is allowed");
		}

		Instance = this;

		AkizukiLog.Information("Loading Packages");
		foreach (var idxFile in new FileEnumerator(idxDir, "*.idx")) {
			AkizukiLog.Information("Opening {Index}", Path.GetFileNameWithoutExtension(idxFile));
			var pkg = new PackageFileSystem(pkgDir, idxFile, validate);
			Packages.Add(pkg);

			var index = Packages.Count - 1;
			foreach (var (id, path) in pkg.Paths) {
				if (pkg.FindFile(id) is not { } file) {
					continue;
				}

				PathLookup[path] = id;
				ReversePathLookup[id] = path;
				IdLookup[id] = (index, path, file);
			}
		}

		if (OpenFile("res/content/assets.bin") is not { } assetsBin) {
			AkizukiLog.Warning("No assets database, ship building will be unavailable");
			return;
		}

		AkizukiLog.Information("Loading Asset Database");
		Database = new BigWorldDatabase(assetsBin, validate);

		if (OpenFile("res/content/GameParams.data") is not { } gameParamsData) {
			AkizukiLog.Warning("No GameParams.data, automatic ship building will be unavailable");
			return;
		}

		AkizukiLog.Information("Loading Game Params data");
		GameParams = new PickledData(gameParamsData);
	}

	public static ResourceManager? Instance { get; private set; }

	public PickledData? GameParams { get; set; }
	public List<PackageFileSystem> Packages { get; set; } = [];
	public Dictionary<string, ulong> PathLookup { get; set; } = [];
	public Dictionary<ulong, string> ReversePathLookup { get; set; } = [];
	public Dictionary<ulong, (int Index, string Name, PFSFile File)> IdLookup { get; set; } = [];
	public IEnumerable<ulong> Files => IdLookup.Keys;
	public BigWorldDatabase? Database { get; set; }

	public void Dispose() {
		foreach (var package in Packages) {
			package.Dispose();
		}

		Packages.Clear();
		Instance = null;
	}

	public IPrototype? OpenPrototype(string path) => Database?.Resolve(path);

	public IPrototype? OpenPrototype(ulong id) => Database?.Resolve(id);

	public IMemoryBuffer<byte>? OpenFile(string path) {
		if (PathLookup.TryGetValue(path, out var id)) {
			return OpenFile(id);
		}

		AkizukiLog.Debug("Could not find {Path}", path);
		return null;
	}

	public IMemoryBuffer<byte>? OpenFile(ulong id) {
		if (IdLookup.TryGetValue(id, out var pair)) {
			return Packages[pair.Index].OpenFile(pair.File);
		}

		AkizukiLog.Debug("Could not find {Id:x16}", id);
		return null;
	}
}
