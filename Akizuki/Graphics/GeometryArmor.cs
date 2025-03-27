using Akizuki.Structs.Graphics;
using DragonLib.IO;

namespace Akizuki.Graphics;

public sealed class GeometryArmor : IDisposable {
	public GeometryArmor(GeometrySimpleBufferHeader header, MemoryReader reader) {
		var pos = reader.Offset;
		reader.Offset = (int) (pos + header.NamePtr + 8);
		Name = reader.ReadString((int) (header.NameLength - 1));

		reader.Offset = (int) (pos + header.BufferPtr);

		var armorHeader = reader.Read<GeometryArmorHeader>();
		Id = armorHeader.Id;
		BoundingBox = armorHeader.BoundingBox;

		for (var index = 0; index < armorHeader.PlateCount; index++) {
			Plates.Add(new GeometryArmorPlate(reader.Read<GeometryArmorPlateHeader>(), reader));
		}
	}

	public string Name { get; set; }
	public uint Id { get; set; }
	public BoundingBox BoundingBox { get; set; }
	public List<GeometryArmorPlate> Plates { get; set; } = [];

	public void Dispose() {
		foreach (var plate in Plates) {
			plate.Dispose();
		}
	}
}
