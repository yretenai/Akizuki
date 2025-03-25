// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using System.Text.Json.Serialization;
using Akizuki.Data;

namespace Akizuki.Json;

public class JsonPackageFileSystemConverter : JsonConverter<PackageFileSystem> {
	public override PackageFileSystem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

	public override void Write(Utf8JsonWriter writer, PackageFileSystem value, JsonSerializerOptions options) {
		writer.WriteStartObject();

		foreach (var (assetId, path) in value.Paths.OrderBy(x => x.Value)) {
			if (!value.IsAssetIdUsed(assetId)) {
				continue;
			}

			writer.WritePropertyName(assetId.ToString("x016"));
			writer.WriteStringValue(path);
		}

		writer.WriteEndObject();
	}
}
