// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Akizuki.Json;
using Akizuki.Json.Silk;

namespace Akizuki.Conversion.Utility;

internal static class JsonOptions {
	internal static JsonSerializerOptions Options { get; } = new() {
		WriteIndented = true,
		NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		NewLine = "\n",
		Converters = {
			new JsonStringEnumConverter(),
			new JsonBigWorldDatabaseConverter(),
			new JsonPackageFileSystemConverter(),
			new JsonStringIdConverter(),
			new JsonResourceIdConverter(),
			new JsonMatrix4X4ConverterFactory(),
			new JsonVector2DConverterFactory(),
			new JsonVector3DConverterFactory(),
			new JsonVector4DConverterFactory(),
		},
	};

	internal static JsonSerializerOptions FromXmlOptions { get; } = new() {
		WriteIndented = true,
		NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
		IgnoreReadOnlyProperties = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		NewLine = "\n",
		Converters = {
			new JsonStringEnumConverter(),
			new JsonBigWorldDatabaseConverter(),
			new JsonPackageFileSystemConverter(),
			new JsonStringIdConverter(),
			new JsonResourceIdConverter(),
			new JsonMatrix4X4ConverterFactory(),
			new JsonVector2DConverterFactory(),
			new JsonVector3DConverterFactory(),
			new JsonVector4DConverterFactory(),
		},
	};

	internal static JsonSerializerOptions SafeOptions { get; } = new() {
		WriteIndented = true,
		NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		NewLine = "\n",
		Converters = {
			new JsonFauxDictionaryConverter(),
			new JsonMatrix4X4ConverterFactory(),
			new JsonVector2DConverterFactory(),
			new JsonVector3DConverterFactory(),
			new JsonVector4DConverterFactory(),
		},
	};

	internal static JsonSerializerOptions GltfOptions =>
		new() {
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true,
			NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		};
}
