// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using System.Text.Json.Serialization;
using Akizuki.Data;

namespace Akizuki.Json;

public class JsonBigWorldDatabaseConverter : JsonConverter<BigWorldDatabase> {
	public override BigWorldDatabase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException("please implement me \ud83e\udd7a");

	public override void Write(Utf8JsonWriter writer, BigWorldDatabase value, JsonSerializerOptions options) {
		writer.WriteStartObject();
		{
			writer.WritePropertyName("Paths");
			writer.WriteStartObject();

			foreach (var (assetId, path) in value.Paths.OrderBy(x => x.Value)) {
				if (!value.IsAssetIdUsed(assetId)) {
					continue;
				}

				writer.WritePropertyName(assetId.ToString("x016"));
				writer.WriteStringValue(path);
			}

			writer.WriteEndObject();

			writer.WritePropertyName("Names");
			writer.WriteStartObject();

			foreach (var (nameId, name) in value.Strings.OrderBy(x => x.Value)) {
				writer.WritePropertyName(nameId.ToString("x8"));
				writer.WriteStringValue(name);
			}

			writer.WriteEndObject();
		}
		writer.WriteEndObject();
	}
}
