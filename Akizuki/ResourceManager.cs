// SPDX-FileCopyrightText: 2026 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Globalization;
using Akizuki.Camouflage;
using Akizuki.Moo;
using Akizuki.PackageFileSystem;
using DragonLib.IO.Binary;
using DragonLib.IO.FileSystem;

namespace Akizuki;

public sealed class ResourceManager : IDisposable {
	public ResourceManager(string installDir, int version = 0, bool validate = false) {
		if (Instance != null) {
			throw new InvalidOperationException(
				"Only one instance of ResourceManager is allowed, call dispose on the previous instance");
		}

		Instance = this;

		if (version == 0) {
		#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
			version = Directory
					  .EnumerateDirectories(Path.Combine(installDir, "bin"), "*", SearchOption.TopDirectoryOnly)
					  .Select(Path.GetFileName)
					  .Where(x => x != default && x.All(char.IsDigit))
					  .Select(int.Parse)
					  .Max();
		#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
		}

		var binDir = Path.Combine(installDir, "bin", version.ToString(CultureInfo.InvariantCulture));
		var idxDir = Path.Combine(binDir, "idx");

		AkizukiLog.Information("Loading Packages");
		foreach (var idxFile in new FileEnumerator(idxDir, "*.idx")) {
			AkizukiLog.Information("Opening {Index}", Path.GetFileNameWithoutExtension(idxFile));

			var idxName = $"idx/{Path.GetFileNameWithoutExtension(idxFile)}.idx";
			var idxId = new ResourceId(idxName);
			ResourceId.Lookup[idxId] = idxName;

			using var stream = new FileStream(idxFile, FileMode.Open, FileAccess.Read, FileShare.Read);
			var pkg = BigWorldFile.OpenByVersion(installDir, stream, validate);
			if (pkg is not Package package) {
				pkg?.Dispose();
				AkizukiLog.Error("{Index} is not a recognized package file", idxFile);
				continue;
			}

			Packages.Add(idxId, package);

			foreach (var path in package.PresentResources) {
				if (!PathLookup.TryAdd(path, path)) {
					continue;
				}

				ReversePathLookup[path] = path;
				ResourceLookup[path] = idxId;
			}
		}

		var locDir = Path.Combine(binDir, "res/texts");
		if (Directory.Exists(locDir)) {
			foreach (var locFile in new FileEnumerator(locDir,
						 new EnumerationOptions
							 { MatchType = MatchType.Simple, RecurseSubdirectories = true }, "*.mo")) {
				var lang = Path.GetFileName(Path.GetDirectoryName(Path.GetFullPath(Path.Combine(locFile, "../../")))) ??
					"xx";
				AkizukiLog.Information("Loading Translation {Lang}", lang);
				using var stream = new FileStream(locFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				Texts[lang] = new MessageObject(stream);
			}
		} else {
			AkizukiLog.Warning("Could not load languages");
		}

		if (OpenResource("res/camouflages.xml") is { } camouflagesXml) {
			AkizukiLog.Information("Loading Camouflage Data");
			Camouflages = new CamouflageData(camouflagesXml);
		} else {
			AkizukiLog.Warning("Could not load Camouflage Data");
		}

		// if (OpenFile("res/content/assets.bin") is not { } assetsBin) {
		// 	AkizukiLog.Warning("No assets database, ship building will be unavailable");
		// 	return;
		// }

		// AkizukiLog.Information("Loading Asset Database");
		// Database = new BigWorldDatabase(assetsBin, validate);

		if (OpenResource("res/content/GameParams.data") is not { } gameParamsData) {
			AkizukiLog.Warning("No GameParams.data, automatic ship building will be unavailable");
			return;
		}

		AkizukiLog.Information("Loading Game Params data");
		GameParams = PickledData.Create(gameParamsData);
	}

	public static ResourceManager? Instance { get; private set; }

	public Dictionary<ResourceId, Package> Packages { get; set; } = [];
	public Dictionary<string, ResourceId> PathLookup { get; set; } = [];
	public Dictionary<ResourceId, string> ReversePathLookup { get; set; } = [];
	public Dictionary<ResourceId, ResourceId> ResourceLookup { get; set; } = [];

	public IEnumerable<ResourceId> Resources => ResourceLookup.Keys;

	// public BigWorldDatabase? Database { get; set; }
	public PickleObject GameParams { get; set; } = [];
	public CamouflageData? Camouflages { get; set; }
	public Dictionary<string, MessageObject> Texts { get; } = new(StringComparer.OrdinalIgnoreCase);

	public void Dispose() {
		// Database.Dispose();

		foreach (var package in Packages.Values) {
			package.Dispose();
		}

		Packages.Clear();
		Instance = null;
	}

	// public IPrototype? OpenPrototype(ResourceId id) => Database?.Resolve(id);
	// public IPrototype? OpenPrototype(string path) => Database?.Resolve(path);
	//
	// public IPrototype? OpenPrototype(ulong id) => Database?.Resolve(id);
	public RentedArray<byte>? OpenResource(ResourceId id) {
		if (!id.IsValid) {
			return null;
		}

		if (ResourceLookup.TryGetValue(id, out var packageId) && Packages[packageId].OpenResource(id) is { } buffer) {
			return buffer;
		}

		AkizukiLog.Debug("Could not find {Id:x16}", id);
		return null;
	}

	public RentedArray<byte>? OpenResource(string path) {
		path = path.TrimStart('/');

		if (!path.StartsWith("res/")) {
			path = "res/" + path;
		}

		if (PathLookup.TryGetValue(path, out var id)) {
			return OpenResource(id);
		}

		AkizukiLog.Debug("Could not find {Path}", path);
		return null;
	}

	public ResourceId FindResource(string path) {
		path = path.TrimStart('/');

		if (!path.StartsWith("res/")) {
			path = "res/" + path;
		}

		return PathLookup.GetValueOrDefault(path, ResourceId.Invalid);
	}

	public bool HasResource(string path) => FindResource(path).IsValid;
}
