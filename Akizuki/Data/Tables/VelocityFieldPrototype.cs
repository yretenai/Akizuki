// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Structs.Data.Tables;
using DragonLib.IO;
using Silk.NET.Maths;

namespace Akizuki.Data.Tables;

public partial class VelocityFieldPrototype {
	public VelocityFieldPrototype(MemoryReader data) {
		var offset = data.Offset;
		var header = data.Read<VelocityFieldPrototypeHeader>();
		Dimensions = header.Dimensions;
		data.Offset = (int) (offset + header.VelocitiesArrayPtr);
		Velocities.AddRange(data.Read<Vector3D<short>>(header.VelocitiesCount / 3));
	}

	public Vector3D<uint> Dimensions { get; set; }
	public List<Vector3D<short>> Velocities { get; set; } = [];
}
