// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace Akizuki.Structs.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public record struct BoundingBox {
	public Vector3D<float> Min { get; set; }
	public Vector3D<float> Max { get; set; }
}
