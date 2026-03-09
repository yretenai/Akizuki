// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using System.Runtime.InteropServices;
using Akizuki.Moo;

namespace Akizuki.PackageFileSystem.V2;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct PackagePathNameV2<TPointer> where TPointer : INumber<TPointer> {
	public PackageResourceNameV2<TPointer> Name { get; set; }
	public ResourceId ParentId { get; set; }
}
