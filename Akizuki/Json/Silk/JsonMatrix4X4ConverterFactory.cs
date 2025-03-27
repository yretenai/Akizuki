// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using System.Text.Json.Serialization;
using Silk.NET.Maths;

namespace Akizuki.Json.Silk;

public class JsonMatrix4X4ConverterFactory : JsonConverterFactory {
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsConstructedGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Matrix4X4<>);
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => (JsonConverter) Activator.CreateInstance(typeof(JsonMatrix4X4Converter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0]))!;

	public class JsonMatrix4X4Converter<T> : JsonConverter<Matrix4X4<T>> where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T> {
		public override Matrix4X4<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException("please implement me \ud83e\udd7a");

		public override void Write(Utf8JsonWriter writer, Matrix4X4<T> value, JsonSerializerOptions options) {
			writer.WriteStartArray();
			JsonSerializer.Serialize(writer, value.Row1, options);
			JsonSerializer.Serialize(writer, value.Row2, options);
			JsonSerializer.Serialize(writer, value.Row3, options);
			JsonSerializer.Serialize(writer, value.Row4, options);
			writer.WriteEndArray();
		}
	}
}
