// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using Akizuki.Moo;
using DragonLib.IO.Binary;

namespace Akizuki.PackageFileSystem.V2;

public class PackageV2<TPointer> : Package where TPointer : INumber<TPointer> {
	public PackageV2(string basePath, Stream stream, bool validateChecksum = false) : base(stream, validateChecksum) {

	}

	public override IRentedArray<byte>? OpenResource(ResourceId resource) => throw new NotImplementedException();
}
