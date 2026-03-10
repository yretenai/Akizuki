using System.Text;
using System.Text.Json.Serialization;
using DragonLib.IO.Binary;

namespace Akizuki.AssetDb;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$prototype")]
// [JsonDerivedType(typeof(MaterialPrototype))]
// [JsonDerivedType(typeof(VisualPrototype))]
// [JsonDerivedType(typeof(ModelPrototype))]
// [JsonDerivedType(typeof(SkeletonPrototype))]
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
	static virtual IPrototype Create(BufferBinaryReader reader) => throw new NotSupportedException();
}
