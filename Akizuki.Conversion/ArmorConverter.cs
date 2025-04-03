// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Akizuki.Conversion.Utility;
using Akizuki.Graphics;
using Akizuki.Structs.Graphics;
using DragonLib.IO;
using GL = GLTF.Scaffold;

namespace Akizuki.Conversion;

public static class ArmorConverter {
	[MethodImpl(MethodConstants.Optimize)]
	public static bool ConvertSplash(string path, IConversionOptions flags, IMemoryBuffer<byte> data) {
		var splash = new Splash(data);

		if (flags.Dry) {
			return true;
		}

		using var stream = new FileStream(path + ".json", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(stream, splash, JsonOptions.Options);
		stream.WriteByte((byte) '\n');
		return true;
	}

	[MethodImpl(MethodConstants.Optimize)]
	public static void CreateArmor(GL.Root gltf, GL.Node node, Stream stream, GeometryArmor armor, Dictionary<int, int> thicknessMaterial) {
		var (meshNode, _) = node.CreateNode(gltf, armor.Name);
		(var mesh, meshNode.Mesh) = gltf.CreateMesh(armor.Name);

		foreach (var plate in armor.Plates) {
			using var indexBuffer = new MemoryBuffer<ushort>(plate.Vertices.Length);
			var view = gltf.CreateBufferView(MemoryMarshal.AsBytes(plate.Vertices.Span), stream, Unsafe.SizeOf<GeometryArmorVertex>(), GL.BufferViewTarget.ArrayBuffer).Id;
			var accessorId = gltf.CreateAccessor(view, plate.Vertices.Length, 0, GL.AccessorType.VEC3, GL.AccessorComponentType.Float).Id;

			if (!thicknessMaterial.TryGetValue(plate.Thickness, out var materialId)) {
				var material = gltf.CreateMaterial($"{plate.Thickness}mm Armor");
				materialId = thicknessMaterial[plate.Thickness] = material.Id;
				material.Material.Extensions = new Dictionary<string, JsonValue> {
					["KHR_materials_unlit"] = JsonValue.Create(new object())!,
				};
				gltf.ExtensionsUsed ??= [];
				gltf.ExtensionsUsed.Add("KHR_materials_unlit");
				material.Material.PBR = new GL.PBRMaterial {
					BaseColorFactor = ColorTheory.LerpColor(ColorTheory.ArmorStart, ColorTheory.ArmorEnd, plate.Thickness / 400.0),
				};
			}

			var primitive = new GL.Primitive {
				Mode = GL.PrimitiveMode.Triangles,
				Material = materialId,
				Attributes = {
					["POSITION"] = accessorId,
				},
			};

			if (plate.Vertices.Length % 3 != 0) {
				throw new InvalidOperationException();
			}

			for (var index = 0; index < plate.Vertices.Length / 3; index += 1) {
				var vert1 = index * 3;
				indexBuffer[vert1] = (ushort) vert1;
				indexBuffer[vert1 + 1] = (ushort) (vert1 + 1);
				indexBuffer[vert1 + 2] = (ushort) (vert1 + 2);
			}

			primitive.Indices = gltf.CreateAccessor(indexBuffer.Span, stream, GL.BufferViewTarget.ElementArrayBuffer, GL.AccessorType.SCALAR, GL.AccessorComponentType.UnsignedShort).Id;

			mesh.Primitives.Add(primitive);
		}
	}
}
