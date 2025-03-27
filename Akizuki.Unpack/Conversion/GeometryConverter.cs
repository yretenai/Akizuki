// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Akizuki.Graphics;
using Akizuki.Structs.Graphics;
using Akizuki.Structs.Graphics.VertexFormat;
using BCDecNet;
using DragonLib.IO;
using Silk.NET.Maths;
using Triton;
using Triton.Encoder;
using Triton.Pixel;
using Triton.Pixel.Channels;
using Triton.Pixel.Formats;
using GL = GLTF.Scaffold;

namespace Akizuki.Unpack.Conversion;

internal static class GeometryConverter {
	private static JsonSerializerOptions GltfOptions =>
		new() {
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true,
			NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		};

	private static MethodInfo VertexMethod { get; } = typeof(GeometryConverter).GetMethod("BuildVertexBuffer", BindingFlags.Static | BindingFlags.NonPublic)!;

	internal static bool ConvertSplash(string path, ProgramFlags flags, IMemoryBuffer<byte> data) {
		var splash = new Splash(data);

		if (flags.Dry) {
			return true;
		}

		using var stream = new FileStream(path + ".json", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(stream, splash, Program.Options);
		stream.WriteByte((byte) '\n');
		return true;
	}

	internal static bool ConvertTexture(string path, ProgramFlags flags, IMemoryBuffer<byte> data) {
		var imageFormat = flags.SelectedFormat;
		if (imageFormat == TextureFormat.None) {
			return false;
		}

		var encoder = flags.FormatEncoder;
		if (encoder == null) {
			return false;
		}

		var ext = Path.GetExtension(path);
		if (ext != ".dds") {
			path = Path.ChangeExtension(path, $".{ext[^1]}.{imageFormat.ToString().ToLowerInvariant()}");
		} else {
			path = Path.ChangeExtension(path, $".{imageFormat.ToString().ToLowerInvariant()}");
		}

		using var texture = new DDSTexture(data);
		if (texture.OneMipSize == 0) {
			return false;
		}

		using var collection = new ImageCollection();

		var width = texture.Width;
		var height = texture.Height;

		var numSurfaces = imageFormat == TextureFormat.TIF ? texture.Surfaces : 1;
		for (var index = 0; index < numSurfaces; ++index) {
			var chunk = texture.GetSurface(index);
			var chunkMem = chunk.Memory;
			if (texture.Format is >= DXGIFormat.BC1_TYPELESS and <= DXGIFormat.BC5_SNORM or >= DXGIFormat.BC6H_TYPELESS and <= DXGIFormat.BC7_UNORM_SRGB) {
				var frameBuffer = new MemoryBuffer<byte>(width * height * 16);
				try {
					var frameBufferMem = frameBuffer.Memory;
					switch (texture.Format) {
						case DXGIFormat.BC1_UNORM:
						case DXGIFormat.BC1_UNORM_SRGB:
							BCDec.DecompressBC1(chunkMem, frameBufferMem, width, height);
							collection.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC2_UNORM:
						case DXGIFormat.BC2_UNORM_SRGB:
							BCDec.DecompressBC2(chunkMem, frameBufferMem, width, height);
							collection.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC3_UNORM:
						case DXGIFormat.BC3_UNORM_SRGB:
							BCDec.DecompressBC3(chunkMem, frameBufferMem, width, height);
							collection.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC4_UNORM:
						case DXGIFormat.BC4_SNORM:
							BCDec.DecompressBC4(chunkMem, frameBufferMem, width, height, texture.Format == DXGIFormat.BC4_SNORM);
							collection.Add(new ImageBuffer<ColorR<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC5_SNORM:
							BCDec.DecompressBC5(chunkMem, frameBufferMem, width, height, true);
							collection.Add(new ImageBuffer<ColorRG<sbyte>, sbyte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC5_UNORM:
							BCDec.DecompressBC5(chunkMem, frameBufferMem, width, height, false);
							collection.Add(new ImageBuffer<ColorRG<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC6H_SF16:
						case DXGIFormat.BC6H_UF16: {
							var isSigned = texture.Format == DXGIFormat.BC6H_SF16;
							BCDec.DecompressBC6HFloat(chunkMem, frameBufferMem, width, height, isSigned);
							collection.Add(new ImageBuffer<ColorRGB<float>, float>(frameBuffer, width, height, isSigned));
							continue;
						}
						case DXGIFormat.BC7_UNORM:
						case DXGIFormat.BC7_UNORM_SRGB:
							BCDec.DecompressBC7(chunkMem, frameBufferMem, width, height);
							collection.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
					}
				} catch {
					frameBuffer.Dispose();
					throw;
				}

				frameBuffer.Dispose();
				throw new UnreachableException();
			}

			switch (texture.Format) {
				case DXGIFormat.A8_UNORM:
				case DXGIFormat.R8_UNORM:
				case DXGIFormat.R8_SNORM:
				case DXGIFormat.P8:
				case DXGIFormat.IA44: // what are these formats 😭
					collection.Add(new ImageBuffer<ColorR<byte>, byte>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8_SINT:
				case DXGIFormat.R8G8_SNORM:
					collection.Add(new ImageBuffer<ColorRG<sbyte>, sbyte>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8_UNORM:
				case DXGIFormat.R8G8_UINT:
				case DXGIFormat.A8P8:
					collection.Add(new ImageBuffer<ColorRG<byte>, byte>(chunk, width, height));
					continue;
				case DXGIFormat.R16_SINT:
				case DXGIFormat.R16_SNORM:
					collection.Add(new ImageBuffer<ColorR<short>, short>(chunk, width, height));
					continue;
				case DXGIFormat.R16_UINT:
				case DXGIFormat.R16_UNORM:
					collection.Add(new ImageBuffer<ColorR<ushort>, ushort>(chunk, width, height));
					continue;
				case DXGIFormat.R16_FLOAT:
					collection.Add(new ImageBuffer<ColorR<Half>, Half>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8B8A8_UNORM:
				case DXGIFormat.R8G8B8A8_UNORM_SRGB:
				case DXGIFormat.R8G8B8A8_UINT:
					collection.Add(new ImageBuffer<ColorRGBA<byte>, byte>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8B8A8_SNORM:
				case DXGIFormat.R8G8B8A8_SINT:
					collection.Add(new ImageBuffer<ColorRGBA<sbyte>, sbyte>(chunk, width, height));
					continue;
				case DXGIFormat.R32_FLOAT:
					collection.Add(new ImageBuffer<ColorR<float>, float>(chunk, width, height));
					continue;
				case DXGIFormat.R32_SINT:
					collection.Add(new ImageBuffer<ColorR<int>, int>(chunk, width, height));
					continue;
				case DXGIFormat.R32_UINT:
					collection.Add(new ImageBuffer<ColorR<uint>, uint>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16_FLOAT:
					collection.Add(new ImageBuffer<ColorRG<Half>, Half>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16_UNORM:
				case DXGIFormat.R16G16_UINT:
					collection.Add(new ImageBuffer<ColorRG<ushort>, ushort>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16_SNORM:
				case DXGIFormat.R16G16_SINT:
					collection.Add(new ImageBuffer<ColorRG<short>, short>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16B16A16_FLOAT:
					collection.Add(new ImageBuffer<ColorRGBA<Half>, Half>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16B16A16_SINT:
				case DXGIFormat.R16G16B16A16_SNORM:
					collection.Add(new ImageBuffer<ColorRGBA<short>, short>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16B16A16_UINT:
				case DXGIFormat.R16G16B16A16_UNORM:
					collection.Add(new ImageBuffer<ColorRGBA<ushort>, ushort>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32_FLOAT:
					collection.Add(new ImageBuffer<ColorRG<float>, float>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32_SINT:
					collection.Add(new ImageBuffer<ColorRG<int>, int>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32_UINT:
					collection.Add(new ImageBuffer<ColorRG<uint>, uint>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32B32A32_FLOAT:
					collection.Add(new ImageBuffer<ColorRGBA<float>, float>(chunk, width, height));
					continue;
				case DXGIFormat.B8G8R8A8_UNORM:
				case DXGIFormat.B8G8R8A8_UNORM_SRGB:
					collection.Add(new ImageBuffer<Color<byte, B, G, R, A>, byte>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8B8_UNORM:
					collection.Add(new ImageBuffer<ColorRGB<byte>, byte>(chunk, width, height));
					continue;
				default:
					throw new NotSupportedException();
			}
		}

		if (flags.Dry) {
			return true;
		}

		using var stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
		encoder.Write(stream, EncoderWriteOptions.Default, collection);
		return true;
	}

	internal static Dictionary<(bool IsVertexBuffer, int GeometryBufferId), Dictionary<string, int>>
		BuildGeometryBuffers(GL.Root gltf, Stream stream, Geometry geometry) {
		var mapping = new Dictionary<(bool, int), Dictionary<string, int>>();

		for (var index = 0; index < geometry.MergedIndexBuffers.Count; index++) {
			var indexBuffer = geometry.MergedIndexBuffers[index];
			var id = gltf.CreateBufferView(indexBuffer.Buffer.Span, stream, -1, GL.BufferViewTarget.ElementArrayBuffer).Id;

			mapping[(false, index)] = new Dictionary<string, int> {
				["INDEX"] = id,
			};
		}

		for (var index = 0; index < geometry.MergedVertexBuffers.Count; index++) {
			var vertexBuffer = geometry.MergedVertexBuffers[index];
			var id = (Dictionary<string, int>) vertexBuffer.CreateVertexGenetic(VertexMethod).Invoke(null, [gltf, stream, vertexBuffer])!;
			if (id.Count == 0) {
				continue;
			}

			mapping[(true, index)] = id;
		}

		return mapping;
	}

	internal static Dictionary<string, int> BuildVertexBuffer<T>(GL.Root gltf, Stream stream, GeometryVertexBuffer indexBuffer) where T : struct, IStandardVertex {
		using var buffer = indexBuffer.DecodeBuffer<T>();
		if (buffer.Length == 0) {
			return [];
		}

		var span = buffer.Span;
		var first = span[0];

		// bone handled elsewhere because remapping.
		using var positions = new MemoryBuffer<Vector3D<float>>(buffer.Length);
		using var normals = new MemoryBuffer<Vector3D<float>>(buffer.Length);
		using var uv1 = new MemoryBuffer<Vector2D<float>>(buffer.Length);
		using IMemoryBuffer<Vector2D<float>> uv2 = first is IUV2Vertex ? new MemoryBuffer<Vector2D<float>>(buffer.Length) : IMemoryBuffer<Vector2D<float>>.Empty;
		using IMemoryBuffer<Vector4D<float>> tangents = first is ITangentVertex ? new MemoryBuffer<Vector4D<float>>(buffer.Length) : IMemoryBuffer<Vector4D<float>>.Empty;
		using IMemoryBuffer<Vector4D<float>> colors = first is IColorVertex ? new MemoryBuffer<Vector4D<float>>(buffer.Length) : IMemoryBuffer<Vector4D<float>>.Empty;

		var positionsSpan = positions.Span;
		var normalsSpan = normals.Span;
		var uv1Span = uv1.Span;
		var uv2Span = uv2.Span;
		var tangentsSpan = tangents.Span;
		var colorsSpan = colors.Span;

		for (var index = 0; index < span.Length; index++) {
			var vertex = span[index];

			positionsSpan[index] = vertex.Position;
			normalsSpan[index] = vertex.Normal;
			uv1Span[index] = vertex.UV;

		#pragma warning disable CA1508
			if (vertex is IUV2Vertex uv2Vert) {
				uv2Span[index] = uv2Vert.UV2;
			}

			if (vertex is ITangentVertex tangentVert) {
				var nor = tangentVert.Normal;
				var tan = tangentVert.Tangent;
				var bin = tangentVert.Binormal;
				var sysNor = new Vector3(nor.X, nor.Y, nor.Z);
				var sysTan = new Vector3(tan.X, tan.Y, tan.Z);
				var sysBin = new Vector3(bin.X, bin.Y, bin.Z);
				var w = Vector3.Dot(Vector3.Cross(sysNor, sysTan), sysBin) < 0 ? -1.0f : 1.0f;

				tangentsSpan[index] = new Vector4D<float>(tan, w);
			}

			if (vertex is IColorVertex color) {
				colorsSpan[index] = color.Color;
			}
		#pragma warning restore CA1508
		}

		var result = new Dictionary<string, int> {
			["POSITION"] = gltf.CreateBufferView(MemoryMarshal.AsBytes(positionsSpan), stream, Unsafe.SizeOf<Vector3D<float>>(), GL.BufferViewTarget.ArrayBuffer).Id,
			["NORMAL"] = gltf.CreateBufferView(MemoryMarshal.AsBytes(normalsSpan), stream, Unsafe.SizeOf<Vector3D<float>>(), GL.BufferViewTarget.ArrayBuffer).Id,
			["TEXCOORD_0"] = gltf.CreateBufferView(MemoryMarshal.AsBytes(uv1Span), stream, Unsafe.SizeOf<Vector2D<float>>(), GL.BufferViewTarget.ArrayBuffer).Id,
		};

		if (uv2Span.Length > 0) {
			result["TEXCOORD_1"] = gltf.CreateBufferView(MemoryMarshal.AsBytes(uv2Span), stream, Unsafe.SizeOf<Vector2D<float>>(), GL.BufferViewTarget.ArrayBuffer).Id;
		}

		if (tangentsSpan.Length > 0) {
			result["TANGENT"] = gltf.CreateBufferView(MemoryMarshal.AsBytes(tangentsSpan), stream, Unsafe.SizeOf<Vector4D<float>>(), GL.BufferViewTarget.ArrayBuffer).Id;
		}

		if (colorsSpan.Length > 0) {
			result["COLOR_0"] = gltf.CreateBufferView(MemoryMarshal.AsBytes(colorsSpan), stream, Unsafe.SizeOf<Vector4D<float>>(), GL.BufferViewTarget.ArrayBuffer).Id;
		}

		return result;
	}

	internal static bool ConvertLooseGeometry(string path, ProgramFlags flags, IMemoryBuffer<byte> data) {
		using var geometry = new Geometry(data);

		var gltfPath = Path.ChangeExtension(path, ".gltf");
		var bufferPath = Path.ChangeExtension(path, ".glbin");
		using Stream bufferStream = flags.Dry ? new MemoryStream() : new FileStream(bufferPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

		var gltf = new GL.Root();

		var root = gltf.CreateNode().Node;
		root.Name = Path.GetFileNameWithoutExtension(path);

		var buffers = BuildGeometryBuffers(gltf, bufferStream, geometry);

		foreach (var (vertexBuffer, indexBuffer) in geometry.SharedVertexBuffers.Values.Zip(geometry.SharedIndexBuffers.Values)) {
			var name = vertexBuffer.Name.Text;
			if (name.EndsWith(".vertices")) {
				name = name[..^9];
			}

			CreateMesh(gltf, root, name, buffers, vertexBuffer, indexBuffer, geometry);
		}

		var thicknessMaterial = new Dictionary<int, int>();
		foreach (var armor in geometry.Armors) {
			CreateArmor(gltf, root, bufferStream, armor, thicknessMaterial);
		}

		gltf.Buffers = [
			new GL.Buffer {
				Uri = Path.GetFileName(bufferPath),
				ByteLength = bufferStream.Length,
			},
		];

		if (flags.Dry) {
			return true;
		}

		using var file = new FileStream(gltfPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(file, gltf, GltfOptions);
		file.Write(Encoding.UTF8.GetBytes(Environment.NewLine));
		return true;
	}


	private static GL.Node CreateArmor(GL.Root gltf, GL.Node node, Stream stream, GeometryArmor armor, Dictionary<int, int> thicknessMaterial) {
		var (meshNode, _) = node.CreateNode(gltf);
		(var mesh, meshNode.Mesh) = gltf.CreateMesh();
		mesh.Name = meshNode.Name = armor.Name;
		var max = (float) armor.Plates.Max(x => x.Thickness);

		foreach (var plate in armor.Plates) {
			using var indexBuffer = new MemoryBuffer<ushort>(plate.Vertices.Length);
			var view = gltf.CreateBufferView(MemoryMarshal.AsBytes(plate.Vertices.Span), stream, Unsafe.SizeOf<GeometryArmorVertex>(), GL.BufferViewTarget.ArrayBuffer).Id;
			var accessorId = gltf.CreateAccessor(view, plate.Vertices.Length, 0, GL.AccessorType.VEC3, GL.AccessorComponentType.Float).Id;

			if (!thicknessMaterial.TryGetValue(plate.Thickness, out var materialId)) {
				var material = gltf.CreateMaterial();
				material.Material.Name = $"{plate.Thickness}mm Armor";
				materialId = thicknessMaterial[plate.Thickness] = material.Id;
				material.Material.Extensions = new Dictionary<string, JsonValue> {
					["KHR_materials_unlit"] = JsonValue.Create(new object())!,
				};
				gltf.ExtensionsUsed ??= [];
				gltf.ExtensionsUsed.Add("KHR_materials_unlit");
				material.Material.PBR = new GL.PBRMaterial {
					BaseColorFactor = ColorTheory.LerpColor(ColorTheory.ArmorStart, ColorTheory.ArmorEnd, plate.Thickness / max),
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

		return meshNode;
	}

	private static GL.Node CreateMesh(GL.Root gltf, GL.Node node, string name,
		Dictionary<(bool IsVertexBuffer, int GeometryBufferId), Dictionary<string, int>> buffers,
		GeometryName vertexBuffer, GeometryName indexBuffer, Geometry geometry) {
		var (meshNode, _) = node.CreateNode(gltf);

		(var mesh, meshNode.Mesh) = gltf.CreateMesh();
		mesh.Name = meshNode.Name = name;

		var mergedVbo = buffers[(true, vertexBuffer.BufferIndex)];
		var mergedIbo = buffers[(false, indexBuffer.BufferIndex)];

		var primitive = new GL.Primitive {
			Mode = GL.PrimitiveMode.Triangles,
		};

		var iboStride = geometry.MergedIndexBuffers[indexBuffer.BufferIndex].Stride;

		primitive.Indices = gltf.CreateAccessor(mergedIbo["INDEX"], indexBuffer.BufferLength,
			indexBuffer.BufferOffset * iboStride, GL.AccessorType.SCALAR,
			iboStride == 2 ? GL.AccessorComponentType.UnsignedShort : GL.AccessorComponentType.UnsignedInt).Id;

		foreach (var (type, bufferIndex) in mergedVbo) {
			var vboStride = gltf.BufferViews![bufferIndex].ByteStride!.Value;
			var (accessorType, accessorComponentType) = type switch {
				"POSITION" => (GL.AccessorType.VEC3, GL.AccessorComponentType.Float),
				"NORMAL" => (GL.AccessorType.VEC3, GL.AccessorComponentType.Float),
				"TEXCOORD_0" => (GL.AccessorType.VEC2, GL.AccessorComponentType.Float),
				"TEXCOORD_1" => (GL.AccessorType.VEC2, GL.AccessorComponentType.Float),
				"TANGENT" => (GL.AccessorType.VEC4, GL.AccessorComponentType.Float),
				"COLOR_0" => (GL.AccessorType.VEC4, GL.AccessorComponentType.Float),
				_ => throw new NotImplementedException(),
			};

			var (_, accessorId) = gltf.CreateAccessor(bufferIndex, vertexBuffer.BufferLength,
				vertexBuffer.BufferOffset * vboStride, accessorType, accessorComponentType);

			primitive.Attributes[type] = accessorId;

			/*
				if (type == "POSITION") {
					accessor.Max = [double.MinValue, double.MinValue, double.MinValue];
					accessor.Min = [double.MaxValue, double.MaxValue, double.MaxValue];
					// todo: calculate bounds
				}
				*/
		}

		mesh.Primitives.Add(primitive);

		return meshNode;
	}
}
