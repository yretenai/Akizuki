using System.Text.Json;
using Akizuki.Data;

namespace Akizuki.Unpack.Conversion;

public static class Assets {
	public static void Save(string root, BigWorldDatabase assets) {
		foreach (var (assetId, prototypeId) in assets.ResourceToPrototype) {
			if (assets.Resolve(prototypeId) is not { } prototype) {
				continue;
			}

			var path = Path.Combine(root, assets.Paths.TryGetValue(assetId, out var name) ? name.TrimStart('/', '.') : $"res/assets/{assetId:x16}.{prototype.GetType().Name}");
			if (!Path.HasExtension(path)) {
				// special edge case for .xml
				path = Path.GetDirectoryName(path) + "." + Path.GetFileName(path);
			}

			path += ".json";

			var dir = Path.GetDirectoryName(path) ?? root;
			Directory.CreateDirectory(dir);

			using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			JsonSerializer.Serialize(stream, prototype, Program.Options);
		}
	}
}
