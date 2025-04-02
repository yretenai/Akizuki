// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Data;
using Akizuki.Data.Tables;
using Akizuki.Structs.Data;
using DragonLib.IO;

namespace Akizuki;

public sealed class ResourceManager : IDisposable {
	public ResourceManager(string installDir, bool validate = false) {
		if (Instance != null) {
			throw new InvalidOperationException("Only one instance of ResourceManager is allowed");
		}

		Instance = this;

		var idxDir = Path.Combine(installDir, "idx");
		var pkgDir = Path.Combine(installDir, "../../res_packages");

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

		var locDir = Path.Combine(installDir, "res/texts");
		if (Directory.Exists(locDir)) {
			foreach (var locFile in new FileEnumerator(locDir, new EnumerationOptions { MatchType = MatchType.Simple, RecurseSubdirectories = true }, "*.mo")) {
				var lang = Path.GetFileName(Path.GetDirectoryName(Path.GetFullPath(Path.Combine(locFile, "../../")))) ?? "xx";
				AkizukiLog.Information("Loading Translation {Lang}", lang);
				using var stream = new FileStream(locFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
				Texts[lang] = new MessageObject(stream);
			}
		} else {
			AkizukiLog.Warning("Could not load languages");
		}

		if (OpenFile("res/camouflages.xml") is { } camouflagesXml) {
			AkizukiLog.Information("Loading Camouflage Data");
			Camouflages = new CamouflageData(camouflagesXml);
		} else {
			AkizukiLog.Warning("Could not load Camouflage Data");
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
		GameParams = PickledData.Create(gameParamsData);
	}


	public static ResourceManager? Instance { get; private set; }

	public List<PackageFileSystem> Packages { get; set; } = [];
	public Dictionary<string, ulong> PathLookup { get; set; } = [];
	public Dictionary<ulong, string> ReversePathLookup { get; set; } = [];
	public Dictionary<ulong, (int Index, string Name, PFSFile File)> IdLookup { get; set; } = [];
	public IEnumerable<ulong> Files => IdLookup.Keys;
	public BigWorldDatabase? Database { get; set; }
	public PickleObject GameParams { get; set; } = [];
	public CamouflageData? Camouflages { get; set; }
	public Dictionary<string, MessageObject> Texts { get; } = new(StringComparer.OrdinalIgnoreCase);

	public void Dispose() {
		foreach (var package in Packages) {
			package.Dispose();
		}

		Packages.Clear();
		Instance = null;
	}

	public IPrototype? OpenPrototype(ResourceId id) => Database?.Resolve(id);
	public IPrototype? OpenPrototype(string path) => Database?.Resolve(path);

	public IPrototype? OpenPrototype(ulong id) => Database?.Resolve(id);
	public IMemoryBuffer<byte>? OpenFile(ResourceId id) => OpenFile(id.Hash);

	public IMemoryBuffer<byte>? OpenFile(string path) {
		path = path.TrimStart('/');

		if (!path.StartsWith("res/")) {
			path = "res/" + path;
		}

		if (PathLookup.TryGetValue(path, out var id)) {
			return OpenFile(id);
		}

		AkizukiLog.Debug("Could not find {Path}", path);
		return null;
	}

	public bool HasFile(string path) {
		path = path.TrimStart('/');

		if (!path.StartsWith("res/")) {
			path = "res/" + path;
		}

		return PathLookup.ContainsKey(path);
	}

	public IMemoryBuffer<byte>? OpenFile(ulong id) {
		if (id is 0 or 0xFFFFFFFFFFFFFFFF) {
			return null;
		}

		if (IdLookup.TryGetValue(id, out var pair)) {
			return Packages[pair.Index].OpenFile(pair.File);
		}

		AkizukiLog.Debug("Could not find {Id:x16}", id);
		return null;
	}
}
