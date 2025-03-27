using System.Text.Json;
using System.Text.Json.Serialization;

namespace Akizuki.Json.Silk;

public class JsonFauxDictionaryConverter : JsonConverter<Dictionary<object, object>> {
	public override Dictionary<object, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

	public override void Write(Utf8JsonWriter writer, Dictionary<object, object> dict, JsonSerializerOptions options) {
		writer.WriteStartArray();

		foreach(var (key, value) in dict) {
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
