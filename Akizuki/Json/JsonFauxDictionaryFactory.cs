// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Akizuki.Json;

public class JsonFauxDictionaryConverter : JsonConverter<Dictionary<object, object>> {
	public override Dictionary<object, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

	public override void Write(Utf8JsonWriter writer, Dictionary<object, object> dict, JsonSerializerOptions options) {
		if (dict.Keys.All(x => x is string or sbyte or short or int or long or byte or ushort or uint or ulong)) {
			writer.WriteStartObject();

			foreach (var (key, value) in dict) {
				var str = key.ToString()!;
				if (str.StartsWith("__") && str.EndsWith("__")) {
					continue;
				}

				writer.WritePropertyName(key.ToString()!);
				JsonSerializer.Serialize(writer, value, options);
			}

			writer.WriteEndObject();

			return;
		}

		writer.WriteStartArray();

		foreach (var (key, value) in dict) {
			writer.WriteStartObject();

			writer.WritePropertyName("Key");
			JsonSerializer.Serialize(writer, key, options);

			writer.WritePropertyName("Value");
			JsonSerializer.Serialize(writer, value, options);

			writer.WriteEndObject();
		}

		writer.WriteEndArray();
	}
}
