// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using Akizuki.Structs.Data;
using Akizuki.Structs.Data.Tables;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class ModelPrototype {
	public ModelPrototype(MemoryReader data) {
		var offset = data.Offset;
		var header = data.Read<ModelPrototypeHeader>();
		VisualResource = header.VisualResourceId;
		MiscType = header.MiscType;

		data.Offset = (int) (offset + header.AnimationsPtr);
		var ids = data.Read<ResourceId>(header.AnimationsCount);
		Animations.AddRange(ids);

		var dyesOffset = data.Offset = (int) (offset + header.DyesPtr);
		var dyes = data.Read<DyePrototypeHeader>(header.DyesCount);
		var oneDye = Unsafe.SizeOf<DyePrototypeHeader>();
		for (var index = 0; index < header.DyesCount; ++index) {
			data.Offset = dyesOffset;
			Dyes.Add(new DyePrototype(dyes[index], data));
			dyesOffset += oneDye;
		}
	}

	public ResourceId VisualResource { get; set; }
	public ModelMiscType MiscType { get; set; }
	public List<ResourceId> Animations { get; set; } = [];
	public List<DyePrototype> Dyes { get; set; } = [];
}
