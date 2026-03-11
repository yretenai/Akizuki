// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Moo;
using DragonLib.IO.Binary;

namespace Akizuki.AssetDb;

public class AssetDatabase : BigWorldFile {
	public AssetDatabase(BufferBinaryReader reader, bool validateChecksum = false) : base(reader, validateChecksum) {
		throw new NotImplementedException();
	}

	public IEnumerable<ResourceId> PresentResources => [];

	public IPrototype? OpenPrototype(ResourceId resource) {
		throw new NotImplementedException();
	}
}
