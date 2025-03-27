// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using Akizuki.Structs.Data.Tables;
using DragonLib.IO;

namespace Akizuki.Data.Tables;

public partial class EffectMetadataPrototype : Dictionary<string, EffectMetaPrototype> {
	public EffectMetadataPrototype(MemoryReader data) {
		var offset = data.Offset;
		var info = data.Read<long>(2);
		var count = (int) info[0];
		offset += (int) info[1];
		data.Offset = offset;

		var metas = data.Read<EffectMetadataPrototypeHeader>(count);
		var oneMeta = Unsafe.SizeOf<EffectMetadataPrototypeHeader>();
		for (var index = 0; index < count; ++index) {
			var meta = metas[index];
			data.Offset = (int) (offset + meta.NamePtr);
			var name = data.ReadString(meta.NameLength - 1);
			this[name] = meta.Metadata;
			offset += oneMeta;
		}
	}
}
