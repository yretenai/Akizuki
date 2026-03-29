// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;
using Pluto.IO.Binary;

namespace Akizuki.AssetDb;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$prototype")]
// [JsonDerivedType(typeof(MaterialPrototype))]
// [JsonDerivedType(typeof(VisualPrototype))]
// [JsonDerivedType(typeof(ModelPrototype))]
// [JsonDerivedType(typeof(SkeletonPrototype))]
// [JsonDerivedType(typeof(SkeletonExtenderPrototype))]
// [JsonDerivedType(typeof(PointLightPrototype))]
// [JsonDerivedType(typeof(VelocityFieldPrototype))]
// [JsonDerivedType(typeof(AtlasContourPrototype))]
// [JsonDerivedType(typeof(EffectPrototype))]
// [JsonDerivedType(typeof(EffectPresetPrototype))]
// [JsonDerivedType(typeof(EffectMetadataPrototype))]
public interface IPrototype {
	static virtual uint Version => 0;
	static virtual uint Id => 0;
	static virtual int Size => 0;
	static virtual string PrototypeName => string.Empty;
	static virtual bool IsSupported(uint version) => false;
	static virtual IEnumerable<IPrototype> Create(uint version, BufferBinaryReader reader) => throw new NotSupportedException();
}
