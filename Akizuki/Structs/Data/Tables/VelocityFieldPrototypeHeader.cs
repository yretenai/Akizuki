// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace Akizuki.Structs.Data.Tables;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct VelocityFieldPrototypeHeader {
	public Vector3D<uint> Dimensions { get; set; }
	public int VelocitiesCount { get; set; }
	public long VelocitiesArrayPtr { get; set; }
}
