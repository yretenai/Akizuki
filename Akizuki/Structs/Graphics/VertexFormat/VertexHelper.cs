// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Silk.NET.Maths;

namespace Akizuki.Structs.Graphics.VertexFormat;

public static class VertexHelper {
	public static float Norm(sbyte value) => (value ^ 0xFF) / 127.0f * 2.0f - 1.0f;

	public static float Norm(byte value) => (value ^ 0xFF) / 255.0f;

	public static Vector3D<float> Unpack(Vector4D<sbyte> packed) => new(Norm(packed.X), Norm(packed.Y), Norm(packed.Z));
	public static Vector4D<float> Unpack(Vector4D<byte> packed) => new(Norm(packed.X), Norm(packed.Y), Norm(packed.Z), Norm(packed.W));
}
