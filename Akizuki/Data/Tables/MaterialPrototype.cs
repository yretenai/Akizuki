// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Data;
using Akizuki.Structs.Data.Tables;
using DragonLib.IO;
using Silk.NET.Maths;

namespace Akizuki.Data.Tables;

public partial class MaterialPrototype {
	public MaterialPrototype(MemoryReader reader) {
		var offset = reader.Offset;
		var header = reader.Read<MaterialPrototypeHeader>();
		FxPath = header.FxPathId;
		CollisionFlags = header.CollisionFlags;
		SortOrder = header.SortOrder;

		reader.Offset = (int) (offset + header.PropertyNameIdsPtr);
		var propertyNameIds = reader.Read<StringId>(header.PropertyCount);
		reader.Offset = (int) (offset + header.PropertyIdsPtr);
		var propertyIds = reader.Read<MaterialPropertyId>(header.PropertyCount);

		reader.Offset = (int) (offset + header.BoolValuesPtr);
		var bools = reader.Read<bool>(header.BoolValuesCount);

		reader.Offset = (int) (offset + header.IntValuesPtr);
		var ints = reader.Read<int>(header.IntValuesCount);

		reader.Offset = (int) (offset + header.UIntValuesPtr);
		var uints = reader.Read<uint>(header.UIntValuesCount);

		reader.Offset = (int) (offset + header.FloatValuesPtr);
		var floats = reader.Read<float>(header.FloatValuesCount);

		reader.Offset = (int) (offset + header.TextureValuesPtr);
		var textures = reader.Read<ResourceId>(header.TextureValuesCount);

		reader.Offset = (int) (offset + header.Vector2ValuesPtr);
		var vec2ds = reader.Read<Vector2D<float>>(header.Vector2ValuesCount);

		reader.Offset = (int) (offset + header.Vector3ValuesPtr);
		var vec3ds = reader.Read<Vector3D<float>>(header.Vector3ValuesCount);

		reader.Offset = (int) (offset + header.Vector4ValuesPtr);
		var vec4ds = reader.Read<Vector4D<float>>(header.Vector4ValuesCount);

		reader.Offset = (int) (offset + header.MatrixValuesPtr);
		var mats = reader.Read<Matrix4X4<float>>(header.MatrixValuesCount);

		for (var index = 0; index < header.PropertyCount; ++index) {
			var propertyInfo = propertyIds[index];
			var propertyName = propertyNameIds[index];

			switch (propertyInfo.Type) {
				case MaterialPropertyType.Bool: {
					BoolValues[propertyName] = bools[propertyInfo.Index];
					break;
				}
				case MaterialPropertyType.Int: {
					IntValues[propertyName] = ints[propertyInfo.Index];
					break;
				}
				case MaterialPropertyType.UInt: {
					UIntValues[propertyName] = uints[propertyInfo.Index];
					break;
				}
				case MaterialPropertyType.Float: {
					FloatValues[propertyName] = floats[propertyInfo.Index];
					break;
				}
				case MaterialPropertyType.Texture: {
					TextureValues[propertyName] = textures[propertyInfo.Index];
					break;
				}
				case MaterialPropertyType.Vector2: {
					Vector2Values[propertyName] = vec2ds[propertyInfo.Index];
					break;
				}
				case MaterialPropertyType.Vector3: {
					Vector3Values[propertyName] = vec3ds[propertyInfo.Index];
					break;
				}
				case MaterialPropertyType.Vector4: {
					Vector4Values[propertyName] = vec4ds[propertyInfo.Index];
					break;
				}
				case MaterialPropertyType.Matrix: {
					MatrixValues[propertyName] = mats[propertyInfo.Index];
					break;
				}
				default: {
					AkizukiLog.Warning("Unrecognized material type {Type}", propertyInfo.Type);
					break;
				}
			}
		}
	}

	public Dictionary<StringId, bool> BoolValues { get; } = [];
	public Dictionary<StringId, int> IntValues { get; } = [];
	public Dictionary<StringId, uint> UIntValues { get; } = [];
	public Dictionary<StringId, float> FloatValues { get; } = [];
	public Dictionary<StringId, ResourceId> TextureValues { get; } = [];
	public Dictionary<StringId, Vector2D<float>> Vector2Values { get; } = [];
	public Dictionary<StringId, Vector3D<float>> Vector3Values { get; } = [];
	public Dictionary<StringId, Vector4D<float>> Vector4Values { get; } = [];
	public Dictionary<StringId, Matrix4X4<float>> MatrixValues { get; } = [];
	public ResourceId FxPath { get; }
	public uint CollisionFlags { get; }
	public int SortOrder { get; }
}
