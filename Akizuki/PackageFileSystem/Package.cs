// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Moo;
using DragonLib.IO.Binary;

namespace Akizuki.PackageFileSystem;

public abstract class Package : BigWorldFile {
	protected Package(Stream stream, bool validateChecksum = false) : base(stream, validateChecksum) { }

	public abstract RentedArray<byte>? OpenResource(ResourceId resource);
}
