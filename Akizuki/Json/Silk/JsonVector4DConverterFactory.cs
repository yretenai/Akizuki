// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using System.Text.Json.Serialization;
using Silk.NET.Maths;

namespace Akizuki.Json.Silk;

public class JsonVector4DConverterFactory : JsonConverterFactory {
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsConstructedGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Vector4D<>);
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => (JsonConverter) Activator.CreateInstance(typeof(JsonVector4DConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0]))!;

	public class JsonVector4DConverter<T> : JsonConverter<Vector4D<T>> where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T> {
		public override Vector4D<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException("please implement me \ud83e\udd7a");

		public override void Write(Utf8JsonWriter writer, Vector4D<T> value, JsonSerializerOptions options) {
			writer.WriteStartArray();
			JsonSerializer.Serialize(writer, value.X, options);
			JsonSerializer.Serialize(writer, value.Y, options);
			JsonSerializer.Serialize(writer, value.Z, options);
			JsonSerializer.Serialize(writer, value.W, options);
			writer.WriteEndArray();
		}
	}
}
