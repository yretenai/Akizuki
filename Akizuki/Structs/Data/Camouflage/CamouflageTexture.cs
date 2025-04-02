// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Globalization;
using System.Xml.Linq;
using System.Xml.Serialization;
using Silk.NET.Maths;

namespace Akizuki.Structs.Data.Camouflage;

[XmlRoot("Texutre")]
public record CamouflageTexture {
	public CamouflageTexture(XElement texture) {
		Path = texture.Value.Trim();

		if (texture.Element("Influence_m") is not { } xInfluenceM ||
			!float.TryParse(xInfluenceM.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var influenceM)) {
			influenceM = 0f;
		}

		if (texture.Element("Influence_g") is not { } xInfluenceG ||
			!float.TryParse(xInfluenceG.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var influenceG)) {
			influenceG = 0f;
		}

		if (texture.Element("Influence_N") is not { } xInfluenceN ||
			!float.TryParse(xInfluenceN.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var influenceN)) {
			influenceN = 0f;
		}

		if (texture.Element("Influence_ao") is not { } xInfluenceAO ||
			!float.TryParse(xInfluenceAO.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var influenceAO)) {
			influenceAO = 0f;
		}

		if (influenceM > 0 || influenceG > 0 || influenceN > 0 || influenceAO > 0) {
			Influence = new Vector4D<float>(influenceM, influenceG, influenceN, influenceAO);
		}

		if (texture.Element("useCamoMaskGlobal") is { } xUseCamoMaskGlobal &&
			bool.TryParse(xUseCamoMaskGlobal.Value, out var useCamoMaskGlobal)) {
			UseCamoMaskGlobal = useCamoMaskGlobal;
		}

		if (texture.Element("camoMode") is { } xCamoMode &&
			int.TryParse(xCamoMode.Value, out var camoMode)) {
			CamoMode = camoMode;
		}

		if (texture.Element("camoEmissionAnimationMode") is { } xCamoEmissionAnimationMode &&
			int.TryParse(xCamoEmissionAnimationMode.Value, out var camoEmissionAnimationMode)) {
			CamoEmissionAnimationMode = camoEmissionAnimationMode;
		}

		if (texture.Element("camoEmissionColorMode") is { } xCamoEmissionColorMode &&
			int.TryParse(xCamoEmissionColorMode.Value, out var camoEmissionColorMode)) {
			CamoEmissionColorMode = camoEmissionColorMode;
		}

		if (texture.Element("camoEmissionBasePower") is { } xCamoEmissionBasePower &&
			float.TryParse(xCamoEmissionBasePower.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var camoEmissionBasePower)) {
			CamoEmissionBasePower = camoEmissionBasePower;
		}

		if (texture.Element("camoEmissionAnimationMaxPower") is { } xCamoEmissionAnimationMaxPower &&
			float.TryParse(xCamoEmissionAnimationMaxPower.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var camoEmissionAnimationMaxPower)) {
			CamoEmissionAnimationMaxPower = camoEmissionAnimationMaxPower;
		}

		if (texture.Element("camoMaskSmooth") is { } xCamoMaskSmooth &&
			float.TryParse(xCamoMaskSmooth.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var camoMaskSmooth)) {
			CamoMaskSmooth = camoMaskSmooth;
		}

		if (texture.Element("camoAnimScaleRaw")?.Value is { } camoAnimScale) {
			CamoAnimScale = CamouflageHelpers.ConvertVec4(camoAnimScale);
		}

		if (texture.Element("camoMaskColor1")?.Value is { } camoMaskColor1) {
			CamoMaskColor1 = CamouflageHelpers.ConvertVec4(camoMaskColor1, 1.0f);
		}

		if (texture.Element("camoMaskColor2Alpha") is not { } xCamoMaskColor2Alpha ||
			!float.TryParse(xCamoMaskColor2Alpha.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var camoMaskColor2Alpha)) {
			camoMaskColor2Alpha = 0f;
		}

		if (texture.Element("camoMaskColor2")?.Value is { } camoMaskColor2) {
			CamoMaskColor2 = CamouflageHelpers.ConvertVec4(camoMaskColor2, camoMaskColor2Alpha > 0 ? camoMaskColor2Alpha : 1.0f);
		} else if (camoMaskColor2Alpha > 0) {
			CamoMaskColor2 = new Vector4D<float>(0, 0, 0, camoMaskColor2Alpha);
		}
	}

	public string Path { get; set; }
	public Vector4D<float>? Influence { get; set; }
	public bool UseCamoMaskGlobal { get; set; }
	public int CamoMode { get; set; }
	public int CamoEmissionAnimationMode { get; set; }
	public int CamoEmissionColorMode { get; set; }
	public float CamoEmissionBasePower { get; set; }
	public float CamoEmissionAnimationMaxPower { get; set; }
	public float CamoMaskSmooth { get; set; }
	public string? CamoMaskSpeedRaw { get; set; }
	public string? CamoMaskColor1Raw { get; set; }
	public string? CamoMaskColor2Raw { get; set; }
	public float? CamoMaskColor2Alpha { get; set; }
	public Vector4D<float>? CamoAnimScale { get; set; }
	public Vector4D<float>? CamoMaskSpeed { get; set; }
	public Vector4D<float>? CamoMaskColor1 { get; set; }
	public Vector4D<float>? CamoMaskColor2 { get; set; }
}
