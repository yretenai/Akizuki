// SPDX-FileCopyrightText: 2026 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Globalization;
using Akizuki.AssetDb;
using Akizuki.Camouflage;
using Akizuki.Moo;
using Akizuki.PackageFileSystem;
using DragonLib.IO.Binary;
using DragonLib.IO.FileSystem;

namespace Akizuki;

public sealed class ResourceManager : IDisposable {
	public ResourceManager(string installDir, int version = 0, bool validate = false) {
		if (Instance != null) {
			throw new InvalidOperationException("Only one instance of ResourceManager is allowed, call dispose on the previous instance");
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

			using var stream = new StreamBinaryReader(idxFile);
			var pkg = BigWorldFile.OpenByVersion(installDir, stream, validate);
			if (pkg is not Package package) {
				pkg?.Dispose();
				AkizukiLog.Error("{Index} is not a recognized package file", idxFile);
				continue;
			}

			Packages.Add(idxId, package);

			foreach (var path in package.PresentResources) {
				if (ResourceLookup.TryAdd(path, idxId)) {
					continue;
				}

				AkizukiLog.Warning("duplicate path {Path}", path);
			}
		}

		var locDir = Path.Combine(binDir, "res/texts");
		if (Directory.Exists(locDir)) {
			foreach (var locFile in new FileEnumerator(locDir, new EnumerationOptions { MatchType = MatchType.Simple, RecurseSubdirectories = true }, "*.mo")) {
				var lang = Path.GetFileName(Path.GetDirectoryName(Path.GetFullPath(Path.Combine(locFile, "../../")))) ?? "xx";
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

		if (OpenResource("res/content/assets.bin") is { } assetsBin) {
			AkizukiLog.Information("Loading Asset Database");
			using var stream = new ArrayPoolBinaryReader(assetsBin);
			var pkg = BigWorldFile.OpenByVersion(installDir, stream, validate);
			if (pkg is AssetDatabase db) {
				Database = db;
			} else {
				pkg?.Dispose();
				AkizukiLog.Error("Could not recognize asset database");
			}
		} else {
			AkizukiLog.Warning("Could not load assets database");
		}

		if (OpenResource("res/content/GameParams.data") is { } gameParamsData) {
			AkizukiLog.Information("Loading Game Params data");
			GameParams = PickledData.Create(gameParamsData);
		} else {
			AkizukiLog.Warning("Could not load game params");
		}
	}

	public static ResourceManager? Instance { get; private set; }

	public Dictionary<ResourceId, Package> Packages { get; set; } = [];
	public Dictionary<ResourceId, ResourceId> ResourceLookup { get; set; } = [];
	public IEnumerable<ResourceId> Resources => ResourceLookup.Keys;
	public AssetDatabase? Database { get; set; }
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

	public IPrototype? OpenPrototype(ResourceId id) {
		if (Database?.OpenPrototype(id) is { } prototype) {
			return prototype;
		}

		AkizukiLog.Debug("Could not find {Id:x16}", id);
		return null;
	}

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
}
