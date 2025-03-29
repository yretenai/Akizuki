// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace Akizuki.Data.Params;

public class ParamTypeInfo {
	public ParamTypeInfo() { }

	public ParamTypeInfo(GameDataObject data) {
		Type = data.GetValue<string>("type");
		Species = data.GetValueOrDefault<string?>("species");
		Nation = data.GetValueOrDefault<string?>("nation");
	}

	public string Type { get; set; } = "Unknown";
	public string? Species { get; set; }
	public string? Nation { get; set; }

	public static string GetTypeName(GameDataObject data) {
		if (data.GetValueOrDefault<GameDataObject>("typeinfo") is { } typeInfo) {
			data = typeInfo;
		}

		return data.GetValueOrDefault("type", string.Empty);
	}
}
