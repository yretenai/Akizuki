// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Xml.Linq;
using Silk.NET.Maths;

namespace Akizuki.Structs.Data.Camouflage;

public record CamouflageColorScheme {
	public CamouflageColorScheme(XElement colorScheme) {
		Name = colorScheme.Element("name")!.Value.Trim();
		Color0 = CamouflageHelpers.ConvertVec4(colorScheme.Element("color0")?.Value);
		Color1 = CamouflageHelpers.ConvertVec4(colorScheme.Element("color1")?.Value);
		Color2 = CamouflageHelpers.ConvertVec4(colorScheme.Element("color2")?.Value);
		Color3 = CamouflageHelpers.ConvertVec4(colorScheme.Element("color3")?.Value);
		if (colorScheme.Element("colorUI")?.Value is { } colorUI) {
			ColorUI = CamouflageHelpers.ConvertVec4(colorUI);
		}
	}

	public string Name { get; set; }
	public Vector4D<float> Color0 { get; set; }

	public Vector4D<float> Color1 { get; set; }

	public Vector4D<float> Color2 { get; set; }

	public Vector4D<float> Color3 { get; set; }

	public Vector4D<float>? ColorUI { get; set; }
}
