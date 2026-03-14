// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using System.Runtime.InteropServices;
using Akizuki.Moo;

namespace Akizuki.PackageFileSystem.V1;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct PackagePathNameV1<TPointer> where TPointer : INumber<TPointer> {
	public ResourceId Id { get; set; }
	public ResourceId ParentId { get; set; }
	public int Length { get; set; }
	public TPointer Offset { get; set; }
}
