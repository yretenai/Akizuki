// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using System.Text.Json.Serialization;
using Akizuki.Structs.Data;

namespace Akizuki.Json;

public class JsonResourceIdConverter : JsonConverter<ResourceId> {
	public override ResourceId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException("please implement me \ud83e\udd7a");

	public override void Write(Utf8JsonWriter writer, ResourceId value, JsonSerializerOptions options) {
		writer.WriteStartObject();
		writer.WriteString("Path", value.Path);
		writer.WriteString("Hash", value.Hash.ToString("x16"));
		writer.WriteEndObject();
	}

	public override void WriteAsPropertyName(Utf8JsonWriter writer, ResourceId value, JsonSerializerOptions options) => writer.WritePropertyName(value.Path.Length == 0 ? value.Hash.ToString("x16") : value.Path);
}
