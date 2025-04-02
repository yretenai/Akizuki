// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Xml.Linq;
using Silk.NET.Maths;

namespace Akizuki.Structs.Data.Camouflage;

public record Camouflage {
	public Camouflage(XElement camouflage) {
		Name = camouflage.Element("name")!.Value.Trim();
		Annotation = camouflage.Element("annotation")?.Value.Trim();
		Realm = camouflage.Element("realm")?.Value.Trim();
		if (camouflage.Element("tiled") is { } xTiled &&
			bool.TryParse(xTiled.Value, out var tiled)) {
			Tiled = tiled;
		}

		if (camouflage.Element("useColorScheme") is { } xUseColorScheme &&
			bool.TryParse(xUseColorScheme.Value, out var useColorScheme)) {
			UseColorScheme = useColorScheme;
		}

		foreach (var targetShip in camouflage.Elements("targetShip")) {
			TargetShips ??= [];
			TargetShips.Add(targetShip.Value.Trim());
		}

		foreach (var colorScheme in camouflage.Elements("colorSchemes")) {
			ColorSchemes ??= [];
			ColorSchemes.Add(colorScheme.Value.Trim());
		}

		if (camouflage.Element("shipGroups") is { } shipGroups) {
			ShipGroups = shipGroups.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();
		}

		if (camouflage.Element("Shaders") is { } shaders) {
			Shaders = [];
			foreach (var shader in shaders.Elements()) {
				Shaders[shader.Name.LocalName] = shader.Value.Trim();
			}
		}

		if (camouflage.Element("UV") is { } uvs) {
			foreach (var uv in uvs.Elements()) {
				UV[uv.Name.LocalName] = CamouflageHelpers.ConvertVec2(uv.Value);
			}
		}

		if (camouflage.Element("Textures") is { } textures) {
			foreach (var texture in textures.Elements()) {
				Textures[texture.Name.LocalName] = new CamouflageTexture(texture);
			}
		}
	}

	public string Name { get; set; }
	public string? Annotation { get; set; }
	public string? Realm { get; set; }
	public bool Tiled { get; set; }
	public bool UseColorScheme { get; set; }
	public List<string>? TargetShips { get; set; }
	public List<string>? ColorSchemes { get; set; }
	public HashSet<string>? ShipGroups { get; set; }
	public Dictionary<string, Vector2D<float>> UV { get; set; } = [];
	public Dictionary<string, string>? Shaders { get; set; }
	public Dictionary<string, CamouflageTexture> Textures { get; set; } = [];
}
