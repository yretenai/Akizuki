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
				Shaders[Enum.Parse<CamouflageShader>(shader.Name.LocalName, true)] = shader.Value.Trim().Replace('\\', '/');
			}
		}

		if (camouflage.Element("UV") is { } uvs) {
			foreach (var uv in uvs.Elements()) {
				UV[Enum.Parse<CamouflagePart>(uv.Name.LocalName, true)] = CamouflageHelpers.ConvertVec2(uv.Value);
			}
		}

		if (camouflage.Element("Textures") is { } textures) {
			foreach (var texture in textures.Elements()) {
				var name = texture.Name.LocalName;
				if (name.EndsWith("_mgn", StringComparison.OrdinalIgnoreCase)) {
					MGNTextures[Enum.Parse<CamouflagePart>(texture.Name.LocalName[..^4], true)] = new CamouflageTexture(texture);
				} else if (name.EndsWith("_animmap", StringComparison.OrdinalIgnoreCase)) {
					AnimMapTextures[Enum.Parse<CamouflagePart>(texture.Name.LocalName[..^8], true)] = new CamouflageTexture(texture);
				} else {
					Textures[Enum.Parse<CamouflagePart>(texture.Name.LocalName, true)] = new CamouflageTexture(texture);
				}
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
	public Dictionary<CamouflagePart, Vector2D<float>> UV { get; set; } = [];
	public Dictionary<CamouflageShader, string>? Shaders { get; set; }
	public Dictionary<CamouflagePart, CamouflageTexture> Textures { get; set; } = [];
	public Dictionary<CamouflagePart, CamouflageTexture> MGNTextures { get; set; } = [];
	public Dictionary<CamouflagePart, CamouflageTexture> AnimMapTextures { get; set; } = [];

	public bool IsValidFor(string name, string ship) {
		if (ShipGroups != null) {
			return false;
		}

		if (name != Name) {
			return false;
		}

		return TargetShips == null || TargetShips.Count == 0 || TargetShips.Contains(ship);
	}
}
