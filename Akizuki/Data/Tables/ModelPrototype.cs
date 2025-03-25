// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using Akizuki.Structs.Data.Tables;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class ModelPrototype {
	private ModelPrototype(MemoryReader data, BigWorldDatabase db) {
		var offset = data.Offset;
		var header = data.Read<ModelPrototypeHeader>();
		VisualResource = db.GetPath(header.VisualResourceId);
		MiscType = header.MiscType;

		data.Offset = (int) (offset + header.AnimationsPtr);
		var ids = data.Read<ulong>(header.AnimationsCount);
		for (var index = 0; index < header.AnimationsCount; ++index) {
			Animations.Add(db.GetPath(ids[index]));
		}

		var dyesOffset = data.Offset = (int) (offset + header.DyesPtr);
		var dyes = data.Read<DyePrototypeHeader>(header.DyesCount);
		var oneDye = Unsafe.SizeOf<DyePrototypeHeader>();
		for (var index = 0; index < header.DyesCount; ++index) {
			data.Offset = dyesOffset;
			Dyes.Add(new DyePrototype(dyes[index], data, db));
			dyesOffset += oneDye;
		}
	}

	public string VisualResource { get; set; }
	public ModelMiscType MiscType { get; set; }
	public List<string> Animations { get; set; } = [];
	public List<DyePrototype> Dyes { get; set; } = [];
}
