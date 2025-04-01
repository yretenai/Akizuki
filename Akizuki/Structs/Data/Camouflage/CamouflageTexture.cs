// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Silk.NET.Maths;

namespace Akizuki.Structs.Data.Camouflage;

[XmlRoot("Texutre")]
public record CamouflageTexture {
	[XmlText]
	public string Path { get; set; } = null!;

	[XmlElement("Influence_m")] [JsonIgnore]
	public float InfluenceM { get; set; }

	[XmlElement("Influence_g")] [JsonIgnore]
	public float InfluenceG { get; set; }

	[XmlElement("Influence_N")] [JsonIgnore]
	public float InfluenceN { get; set; }

	[XmlElement("Influence_ao")] [JsonIgnore]
	public float InfluenceAO { get; set; }

	[XmlElement("useCamoMaskGlobal")]
	public bool UseCamoMaskGlobal { get; set; }

	[XmlElement("camoMode")]
	public int CamoMode { get; set; }

	[XmlElement("camoEmissionAnimationMode")]
	public int CamoEmissionAnimationMode { get; set; }

	[XmlElement("camoEmissionColorMode")]
	public int CamoEmissionColorMode { get; set; }

	[XmlElement("camoEmissionBasePower")]
	public float CamoEmissionBasePower { get; set; }

	[XmlElement("camoEmissionAnimationMaxPower")]
	public float CamoEmissionAnimationMaxPower { get; set; }

	[XmlElement("camoMaskSmooth")]
	public float CamoMaskSmooth { get; set; }

	[XmlElement("camoAnimScale")] [JsonIgnore]
	public string? CamoAnimScaleRaw { get; set; }

	[XmlElement("camoMaskSpeed")] [JsonIgnore]
	public string? CamoMaskSpeedRaw { get; set; }

	[XmlElement("camoMaskColor1")] [JsonIgnore]
	public string? CamoMaskColor1Raw { get; set; }

	[XmlElement("camoMaskColor2")] [JsonIgnore]
	public string? CamoMaskColor2Raw { get; set; }

	[XmlElement("camoMaskColor2Alpha")] [JsonIgnore]
	public float? CamoMaskColor2Alpha { get; set; }

	[XmlIgnore]
	public Vector4D<float>? CamoAnimScale => !string.IsNullOrEmpty(CamoAnimScaleRaw) ? CamouflageRoot.ConvertVec4(CamoAnimScaleRaw) : null;

	[XmlIgnore]
	public Vector4D<float>? CamoMaskSpeed => !string.IsNullOrEmpty(CamoMaskSpeedRaw) ? CamouflageRoot.ConvertVec4(CamoMaskSpeedRaw) : null;

	[XmlIgnore]
	public Vector4D<float>? CamoMaskColor1 => !string.IsNullOrEmpty(CamoMaskColor1Raw) ? CamouflageRoot.ConvertVec4(CamoMaskColor1Raw, 1.0f) : null;

	[XmlIgnore]
	public Vector4D<float>? CamoMaskColor2 => !string.IsNullOrEmpty(CamoMaskColor2Raw) ? CamouflageRoot.ConvertVec4(CamoMaskColor2Raw, CamoMaskColor2Alpha) : null;

	public Vector4D<float>? Influence => InfluenceM > 0 || InfluenceG > 0 || InfluenceN > 0 || InfluenceAO > 0 ? new Vector4D<float>(InfluenceM, InfluenceG, InfluenceN, InfluenceAO) : null;
}
