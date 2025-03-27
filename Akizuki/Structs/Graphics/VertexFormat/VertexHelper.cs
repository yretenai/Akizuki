// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using Silk.NET.Maths;

namespace Akizuki.Structs.Graphics.VertexFormat;

public static class VertexHelper {
	public static float Norm(sbyte value) {
		var unsigned = Unsafe.As<sbyte, byte>(ref value);
		return unsigned > 0x7F ? -((unsigned & 0x7F) / 127.0f) : (unsigned ^ 0x7F) / 127.0f;
	}

	public static float Norm(byte value) => value / 255.0f;

	public static Vector2D<float> Unpack(Vector2D<Half> packed) => new((float) packed.X, (float) packed.Y);

	public static Vector3D<float> Unpack(Vector4D<sbyte> packed) {
		Unsafe.As<Vector4D<sbyte>, uint>(ref packed) ^= uint.MaxValue;
		var result = new Vector3D<float>(Norm(packed.X), Norm(packed.Y), Norm(packed.Z));
		return result;
	}

	public static Vector4D<float> Unpack(Vector4D<byte> packed) {
		Unsafe.As<Vector4D<byte>, uint>(ref packed) ^= uint.MaxValue;
		var result = new Vector4D<float>(Norm(packed.X), Norm(packed.Y), Norm(packed.Z), Norm(packed.W));
		return result;
	}
}
