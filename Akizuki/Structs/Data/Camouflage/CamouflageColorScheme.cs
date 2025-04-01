// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Silk.NET.Maths;

namespace Akizuki.Structs.Data.Camouflage;

public record CamouflageColorScheme {
	[XmlElement("name")]
	public string Name { get; set; } = string.Empty;

	[XmlElement("color0")] [JsonIgnore]
	public string Color0Raw { get; set; } = string.Empty;

	[XmlElement("color1")] [JsonIgnore]
	public string Color1Raw { get; set; } = string.Empty;

	[XmlElement("color2")] [JsonIgnore]
	public string Color2Raw { get; set; } = string.Empty;

	[XmlElement("color3")] [JsonIgnore]
	public string Color3Raw { get; set; } = string.Empty;

	[XmlElement("colorUI")] [JsonIgnore]
	public string? ColorUIRaw { get; set; }

	[XmlIgnore]
	public Vector4D<float> Color0 => CamouflageRoot.ConvertVec4(Color0Raw);

	[XmlIgnore]
	public Vector4D<float> Color1 => CamouflageRoot.ConvertVec4(Color1Raw);

	[XmlIgnore]
	public Vector4D<float> Color2 => CamouflageRoot.ConvertVec4(Color2Raw);

	[XmlIgnore]
	public Vector4D<float> Color3 => CamouflageRoot.ConvertVec4(Color3Raw);

	[XmlIgnore]
	public Vector4D<float>? ColorUI => !string.IsNullOrEmpty(ColorUIRaw) ? CamouflageRoot.ConvertVec4(ColorUIRaw) : null;
}
