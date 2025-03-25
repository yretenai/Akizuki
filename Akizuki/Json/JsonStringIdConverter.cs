// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using System.Text.Json.Serialization;
using Akizuki.Structs.Data;

namespace Akizuki.Json;

public class JsonStringIdConverter : JsonConverter<StringId> {
	public override StringId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException("please implement me \ud83e\udd7a");

	public override void Write(Utf8JsonWriter writer, StringId value, JsonSerializerOptions options) {
		writer.WriteStartObject();
		writer.WriteString("String", value.Text);
		writer.WriteString("Hash", value.Hash.ToString("x8"));
		writer.WriteEndObject();
	}

	public override void WriteAsPropertyName(Utf8JsonWriter writer, StringId value, JsonSerializerOptions options) => writer.WritePropertyName(value.Text.Length == 0 ? value.Hash.ToString("x8") : value.Text);
}
