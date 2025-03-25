// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using System.Text.Json.Serialization;
using Silk.NET.Maths;

namespace Akizuki.Json.Silk;

public class JsonVector2DConverterFactory : JsonConverterFactory {
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsConstructedGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Vector2D<>);
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => (JsonConverter) Activator.CreateInstance(typeof(JsonVector2DConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0]))!;

	public class JsonVector2DConverter<T> : JsonConverter<Vector2D<T>> where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T> {
		public override Vector2D<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException("please implement me \ud83e\udd7a");

		public override void Write(Utf8JsonWriter writer, Vector2D<T> value, JsonSerializerOptions options) {
			writer.WriteStartObject();
			writer.WritePropertyName("X");
			JsonSerializer.Serialize(writer, value.X, options);
			writer.WritePropertyName("Y");
			JsonSerializer.Serialize(writer, value.Y, options);
			writer.WriteEndObject();
		}
	}
}
