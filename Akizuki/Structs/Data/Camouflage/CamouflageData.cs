// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Globalization;
using System.Xml.Serialization;
using Silk.NET.Maths;

namespace Akizuki.Structs.Data.Camouflage;

[XmlRoot("data", IsNullable = false)]
public record CamouflageRoot {
	[XmlArray("ShipGroups")]
	[XmlArrayItem("shipGroup")]
	public List<CamouflageShipGroup> ShipGroups { get; set; } = [];

	[XmlArray("ColorSchemes")]
	[XmlArrayItem("colorScheme")]
	public List<CamouflageColorScheme> ColorSchemes { get; set; } = [];

	[XmlArray("Camouflages")]
	[XmlArrayItem("camouflage")]
	public List<Camouflage> Camouflages { get; set; } = [];

	public static Vector2D<float> ConvertVec2(string value) {
		var values = value.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

		if (values.Length < 1 || !float.TryParse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var u)) {
			u = 1.0f;
		}

		if (values.Length < 2 || !float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) {
			v = 1.0f;
		}

		return new Vector2D<float>(u, v);
	}

	public static Vector4D<float> ConvertVec4(string value) {
		var values = value.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

		if (values.Length < 1 || !float.TryParse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var r)) {
			r = 0.0f;
		}

		if (values.Length < 2 || !float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var g)) {
			g = 0.0f;
		}

		if (values.Length < 3 || !float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var b)) {
			b = 0.0f;
		}

		if (values.Length < 4 || !float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var a)) {
			a = 1.0f;
		}

		return new Vector4D<float>(r, g, b, a);
	}

	public static Vector4D<float> ConvertVec4(string value, float? alpha) {
		var values = value.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

		if (values.Length < 1 || !float.TryParse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var r)) {
			r = 0.0f;
		}

		if (values.Length < 2 || !float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var g)) {
			g = 0.0f;
		}

		if (values.Length < 3 || !float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var b)) {
			b = 0.0f;
		}

		var a = alpha ?? 1.0f;

		return new Vector4D<float>(r, g, b, a);
	}
}
