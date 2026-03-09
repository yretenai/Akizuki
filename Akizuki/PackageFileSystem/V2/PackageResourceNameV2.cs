// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using System.Runtime.InteropServices;
using Akizuki.Moo;

namespace Akizuki.PackageFileSystem.V2;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct PackageResourceNameV2<TPointer> where TPointer : INumber<TPointer>  {
	public TPointer Length { get; set; }
	public TPointer Offset { get; set; }
	public ResourceId Id { get; set; }
}
