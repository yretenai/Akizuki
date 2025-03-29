// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Akizuki.Data.Params;
using Akizuki.Data.Tables;
using Akizuki.Graphics;
using Akizuki.Structs.Data;
using Akizuki.Structs.Graphics;
using Akizuki.Structs.Graphics.VertexFormat;
using BCDecNet;
using DragonLib.IO;
using GLTF.Scaffold.Extensions;
using Silk.NET.Maths;
using Triton;
using Triton.Encoder;
using Triton.Pixel;
using Triton.Pixel.Channels;
using Triton.Pixel.Formats;
using GL = GLTF.Scaffold;
using GeometryCache = System.Collections.Generic.Dictionary<(bool IsVertexBuffer, int GeometryBufferId), System.Collections.Generic.Dictionary<string, int>>;
using PrimitiveCache = System.Collections.Generic.Dictionary<(Akizuki.Structs.Graphics.GeometryName Vertex, Akizuki.Structs.Graphics.GeometryName Index), (System.Collections.Generic.Dictionary<string, int> Attributes, int Indices)>;

namespace Akizuki.Conversion;

public static class GeometryConverter {
	private static JsonSerializerOptions GltfOptions =>
		new() {
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true,
			NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		};

	private static StringId NormalMapId { get; } = new(0x4858745d);
	private static StringId MetalMapId { get; } = new(0x89babfe7);

	// most meshes are flipped on the Z axis when skinned, except a few.
	// this is controlled by SharedVertexBuffer[].Flags but it tends to apply to the entire buffer.
	// todo: populate me with problematic meshes
	public static HashSet<string> FlipBlocklist { get; } = [
		"JGM055_100mm65_Type98",
	];


	public static Dictionary<string, (string Mesh, string Ports)> ResolveShipParts(VisualPrototype prototype, string path) {
		var skeleton = prototype.Skeleton;
		path = Path.ChangeExtension(path, null);
		var result = new Dictionary<string, (string, string)>();
		foreach (var node in skeleton.Names) {
			var name = node.Text;
			if (!name.StartsWith("HP_") || name == "HP_Full") {
				continue;
			}

			var modelPath = path + $"_{name[3..]}";
			result[name] = (modelPath + ".model", modelPath + "_ports.model");
		}

		return result;
	}

	public static string? LocateMiscObject(string id) {
		if (!id.StartsWith("MP_")) {
			return null;
		}

		id = id[3..];

		if (id.Length < 5 || id[1] != 'M') {
			return null;
		}

		var path = "res/content/gameplay/";
		switch (id[0]) {
			case 'A': path += "usa/"; break;
			case 'B': path += "uk/"; break;
			case 'C': path += "common/"; break;
			case 'F': path += "france/"; break;
			case 'G': path += "germany/"; break;
			case 'H': path += "netherlands/"; break;
			case 'I': path += "italy/"; break;
			case 'J': path += "japan/"; break;
			case 'R': path += "russia/"; break;
			case 'S': path += "spain/"; break;
			case 'U': path += "commonwealth/"; break;
			case 'V': path += "panamerica/"; break;
			case 'W': path += "europe/"; break;
			// case 'X': path += "events/"; break;
			case 'Z': path += "panasia/"; break;
			default: return null;
		}

		path += "misc/";

		var underscore = id.IndexOf('_', StringComparison.Ordinal);
		if (underscore > -1) {
			id = id[..underscore];
		}

		path += $"{id}/{id}.model";
		return path;
	}

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

	public static string? ConvertTexture(string path, IConversionOptions flags, IMemoryBuffer<byte> data, bool skipExisting = false, bool isNormalMap = false, bool isMetalGlossMap = false) {
		var imageFormat = flags.SelectedFormat;
		if (imageFormat == TextureFormat.None) {
			return null;
		}

		var encoder = flags.FormatEncoder;
		if (encoder == null) {
			return null;
		}

		var ext = Path.GetExtension(path);
		path = Path.ChangeExtension(path,
			ext != ".dds" ? $".{ext[^1]}.{imageFormat.ToString().ToLowerInvariant()}" : $".{imageFormat.ToString().ToLowerInvariant()}");

		if (skipExisting && File.Exists(path)) {
			return path;
		}

		using var texture = new DDSTexture(data);
		if (texture.OneMipSize == 0) {
			return null;
		}

		var isCubemap = texture.IsCubeMap;

		if (!flags.ConvertTextures && !isCubemap) {
			return null;
		}

		if (!flags.ConvertCubeMaps && isCubemap) {
			return null;
		}

		using var frames = new ImageCollection();

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
					// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
					switch (texture.Format) {
						case DXGIFormat.BC1_UNORM:
						case DXGIFormat.BC1_UNORM_SRGB:
							BCDec.DecompressBC1(chunkMem, frameBufferMem, width, height);
							frames.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC2_UNORM:
						case DXGIFormat.BC2_UNORM_SRGB:
							BCDec.DecompressBC2(chunkMem, frameBufferMem, width, height);
							frames.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC3_UNORM:
						case DXGIFormat.BC3_UNORM_SRGB:
							BCDec.DecompressBC3(chunkMem, frameBufferMem, width, height);
							frames.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC4_UNORM:
						case DXGIFormat.BC4_SNORM:
							BCDec.DecompressBC4(chunkMem, frameBufferMem, width, height, texture.Format == DXGIFormat.BC4_SNORM);
							frames.Add(new ImageBuffer<ColorR<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC5_SNORM:
							BCDec.DecompressBC5(chunkMem, frameBufferMem, width, height, true);
							frames.Add(new ImageBuffer<ColorRG<sbyte>, sbyte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC5_UNORM:
							BCDec.DecompressBC5(chunkMem, frameBufferMem, width, height, false);
							frames.Add(new ImageBuffer<ColorRG<byte>, byte>(frameBuffer, width, height));
							continue;
						case DXGIFormat.BC6H_SF16:
						case DXGIFormat.BC6H_UF16: {
							var isSigned = texture.Format == DXGIFormat.BC6H_SF16;
							BCDec.DecompressBC6HFloat(chunkMem, frameBufferMem, width, height, isSigned);
							frames.Add(new ImageBuffer<ColorRGB<float>, float>(frameBuffer, width, height, isSigned));
							continue;
						}
						case DXGIFormat.BC7_UNORM:
						case DXGIFormat.BC7_UNORM_SRGB:
							BCDec.DecompressBC7(chunkMem, frameBufferMem, width, height);
							frames.Add(new ImageBuffer<ColorRGBA<byte>, byte>(frameBuffer, width, height));
							continue;
					}
				} catch {
					frameBuffer.Dispose();
					throw;
				}

				frameBuffer.Dispose();
				throw new UnreachableException();
			}

			// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
			switch (texture.Format) {
				case DXGIFormat.A8_UNORM:
				case DXGIFormat.R8_UNORM:
				case DXGIFormat.R8_SNORM:
				case DXGIFormat.P8:
				case DXGIFormat.IA44: // what are these formats 😭
					frames.Add(new ImageBuffer<ColorR<byte>, byte>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8_SINT:
				case DXGIFormat.R8G8_SNORM:
					frames.Add(new ImageBuffer<ColorRG<sbyte>, sbyte>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8_UNORM:
				case DXGIFormat.R8G8_UINT:
				case DXGIFormat.A8P8:
					frames.Add(new ImageBuffer<ColorRG<byte>, byte>(chunk, width, height));
					continue;
				case DXGIFormat.R16_SINT:
				case DXGIFormat.R16_SNORM:
					frames.Add(new ImageBuffer<ColorR<short>, short>(chunk, width, height));
					continue;
				case DXGIFormat.R16_UINT:
				case DXGIFormat.R16_UNORM:
					frames.Add(new ImageBuffer<ColorR<ushort>, ushort>(chunk, width, height));
					continue;
				case DXGIFormat.R16_FLOAT:
					frames.Add(new ImageBuffer<ColorR<Half>, Half>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8B8A8_UNORM:
				case DXGIFormat.R8G8B8A8_UNORM_SRGB:
				case DXGIFormat.R8G8B8A8_UINT:
					frames.Add(new ImageBuffer<ColorRGBA<byte>, byte>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8B8A8_SNORM:
				case DXGIFormat.R8G8B8A8_SINT:
					frames.Add(new ImageBuffer<ColorRGBA<sbyte>, sbyte>(chunk, width, height));
					continue;
				case DXGIFormat.R32_FLOAT:
					frames.Add(new ImageBuffer<ColorR<float>, float>(chunk, width, height));
					continue;
				case DXGIFormat.R32_SINT:
					frames.Add(new ImageBuffer<ColorR<int>, int>(chunk, width, height));
					continue;
				case DXGIFormat.R32_UINT:
					frames.Add(new ImageBuffer<ColorR<uint>, uint>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16_FLOAT:
					frames.Add(new ImageBuffer<ColorRG<Half>, Half>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16_UNORM:
				case DXGIFormat.R16G16_UINT:
					frames.Add(new ImageBuffer<ColorRG<ushort>, ushort>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16_SNORM:
				case DXGIFormat.R16G16_SINT:
					frames.Add(new ImageBuffer<ColorRG<short>, short>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16B16A16_FLOAT:
					frames.Add(new ImageBuffer<ColorRGBA<Half>, Half>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16B16A16_SINT:
				case DXGIFormat.R16G16B16A16_SNORM:
					frames.Add(new ImageBuffer<ColorRGBA<short>, short>(chunk, width, height));
					continue;
				case DXGIFormat.R16G16B16A16_UINT:
				case DXGIFormat.R16G16B16A16_UNORM:
					frames.Add(new ImageBuffer<ColorRGBA<ushort>, ushort>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32_FLOAT:
					frames.Add(new ImageBuffer<ColorRG<float>, float>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32_SINT:
					frames.Add(new ImageBuffer<ColorRG<int>, int>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32_UINT:
					frames.Add(new ImageBuffer<ColorRG<uint>, uint>(chunk, width, height));
					continue;
				case DXGIFormat.R32G32B32A32_FLOAT:
					frames.Add(new ImageBuffer<ColorRGBA<float>, float>(chunk, width, height));
					continue;
				case DXGIFormat.B8G8R8A8_UNORM:
				case DXGIFormat.B8G8R8A8_UNORM_SRGB:
					frames.Add(new ImageBuffer<Color<byte, B, G, R, A>, byte>(chunk, width, height));
					continue;
				case DXGIFormat.R8G8B8_UNORM:
					frames.Add(new ImageBuffer<ColorRGB<byte>, byte>(chunk, width, height));
					continue;
				default:
					throw new NotSupportedException();
			}
		}

		if (isMetalGlossMap || isNormalMap) {
			for (var index = 0; index < frames.Count; index++) {
				var image = frames[index];
				var oldImage = default(IImageBuffer);
				ImageBuffer<ColorRGBA<byte>, byte> newImage;
				if (image is ImageBuffer<ColorRGBA<byte>, byte> rgba) {
					newImage = rgba;
				} else {
					oldImage = image;
					newImage = (ImageBuffer<ColorRGBA<byte>, byte>) image.Cast<ColorRGBA<byte>, byte>();
					frames[index] = newImage;
				}

				foreach (ref var pixel in newImage.ColorData.Memory.Span[..(newImage.Width * newImage.Height)]) {
					if (isMetalGlossMap) {
						var roughness = (byte) (0xFF - pixel.R);
						var metalness = pixel.B;
						pixel = new ColorRGBA<byte>(pixel.G, roughness, metalness, pixel.A);
					} else if (isNormalMap) {
						// why
						pixel.R ^= 0xFF;
						pixel.G ^= 0xFF;
						pixel.A = pixel.B; // waterline.
						pixel.B = 0xFF;
					}
				}

				oldImage?.Dispose();
			}
		}

		if (!isCubemap) {
			return SaveTexture(path, flags, encoder, frames);
		}

		using var cubemap = new IBLImage(frames, CubemapOrder.DXGIOrder);
		using var converted = cubemap.Convert(flags.CubemapStyle);
		return SaveTexture(path, flags, encoder, [converted]);
	}

	private static string SaveTexture(string path, IConversionOptions flags, IEncoder encoder, ImageCollection frames) {
		if (flags.Dry) {
			return path;
		}

		using var stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
		encoder.Write(stream, EncoderWriteOptions.Default, frames);
		return path;
	}

	public static GeometryCache
		BuildGeometryBuffers(GL.Root gltf, Stream stream, Geometry geometry, string name) {
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
			var id = BuildVertexBuffer(gltf, stream, vertexBuffer, name);
			if (id.Count == 0) {
				continue;
			}

			mapping[(true, index)] = id;
		}

		return mapping;
	}

	public static void BuildSkinBuffer(GL.Root gltf, Stream stream, GeometryVertexBuffer vertexBuffer, GeometryName vertexBufferName,
		Dictionary<string, int> attributes, RenderSetPrototype renderSet, VisualPrototype visual) {
		var buffer = vertexBuffer.Buffer.Span;
		var info = vertexBuffer.Info;
		if (renderSet.Nodes.Count == 1 || info.BoneIndex == -1 || info.BoneWeight == -1) {
			return;
		}

		var span = buffer.Slice(vertexBufferName.BufferOffset * vertexBuffer.Stride, vertexBufferName.BufferLength * vertexBuffer.Stride);
		using var indices = new MemoryBuffer<Vector4D<ushort>>(buffer.Length);
		using var weights = new MemoryBuffer<Vector4D<float>>(buffer.Length);
		var indicesSpan = indices.Span;
		var weightsSpan = weights.Span;

		var renderBones = renderSet.Nodes;

		for (var index = 0; index < vertexBufferName.BufferLength; index++) {
			var vertex = span.Slice(index * vertexBuffer.Stride, vertexBuffer.Stride);
			var bones = VertexHelper.UnpackBoneIndex(MemoryMarshal.Read<Vector4D<byte>>(vertex[info.BoneIndex..]));
			indicesSpan[index] = new Vector4D<ushort>(RemapBone(bones.X), RemapBone(bones.Y), RemapBone(bones.Z), 0);
			weightsSpan[index] = VertexHelper.UnpackBoneWeight(MemoryMarshal.Read<Vector4D<byte>>(vertex[info.BoneWeight..]));
			continue;

			ushort RemapBone(byte boneIndex) => (ushort) (boneIndex >= renderBones.Count ? 0 : visual.Skeleton.NameMap[renderBones[boneIndex]]);
		}

		attributes["JOINTS_0"] = gltf.CreateAccessor(indicesSpan, stream, GL.BufferViewTarget.ArrayBuffer, GL.AccessorType.VEC4, GL.AccessorComponentType.UnsignedShort).Id;
		attributes["WEIGHTS_0"] = gltf.CreateAccessor(weightsSpan, stream, GL.BufferViewTarget.ArrayBuffer, GL.AccessorType.VEC4, GL.AccessorComponentType.Float).Id;
	}

	public static Dictionary<string, int> BuildVertexBuffer(GL.Root gltf, Stream stream, GeometryVertexBuffer vertexBuffer, string name) {
		var buffer = vertexBuffer.Buffer.Span;
		var info = vertexBuffer.Info;

		// bones not handled here
		using var positions = new MemoryBuffer<Vector3D<float>>(buffer.Length);
		using var normals = new MemoryBuffer<Vector3D<float>>(buffer.Length);
		using var uv1 = new MemoryBuffer<Vector2D<float>>(buffer.Length);
		using IMemoryBuffer<Vector2D<float>> uv2 = info.UV2 > -1 ? new MemoryBuffer<Vector2D<float>>(buffer.Length) : IMemoryBuffer<Vector2D<float>>.Empty;
		using IMemoryBuffer<Vector4D<float>> tangents = info.Tangent > -1 ? new MemoryBuffer<Vector4D<float>>(buffer.Length) : IMemoryBuffer<Vector4D<float>>.Empty;
		using IMemoryBuffer<Vector4D<float>> colors = info.Color > -1 ? new MemoryBuffer<Vector4D<float>>(buffer.Length) : IMemoryBuffer<Vector4D<float>>.Empty;

		var positionsSpan = positions.Span;
		var normalsSpan = normals.Span;
		var uv1Span = uv1.Span;
		var uv2Span = uv2.Span;
		var tangentsSpan = tangents.Span;
		var colorsSpan = colors.Span;

		var shouldFlip = vertexBuffer.Header.IsSkinned && !FlipBlocklist.Contains(name);
		for (var index = 0; index < vertexBuffer.VertexCount; index += 1) {
			var vertex = buffer.Slice(index * vertexBuffer.Stride, vertexBuffer.Stride);

			var pos = MemoryMarshal.Read<Vector3D<float>>(vertex[info.Position..]);
			var nor = VertexHelper.UnpackNormal(MemoryMarshal.Read<Vector4D<sbyte>>(vertex[info.Normal..]));
			if (shouldFlip) {
				pos *= new Vector3D<float>(1, 1, -1);
				nor *= -1;
			}

			positionsSpan[index] = pos;
			normals[index] = nor;
			uv1Span[index] = VertexHelper.UnpackUV(MemoryMarshal.Read<Vector2D<Half>>(vertex[info.UV1..]));

		#pragma warning disable CA1508
			if (info.UV2 > -1) {
				uv2Span[index] = VertexHelper.UnpackUV(MemoryMarshal.Read<Vector2D<Half>>(vertex[info.UV2..]));
			}

			if (info.Tangent > -1) {
				var tan = VertexHelper.UnpackNormal(MemoryMarshal.Read<Vector4D<sbyte>>(vertex[info.Tangent..]));
				var bin = VertexHelper.UnpackNormal(MemoryMarshal.Read<Vector4D<sbyte>>(vertex[info.Binormal..]));
				// todo: should we flip here too?
				var w = Math.Sign(Vector3D.Dot(Vector3D.Cross(nor, tan), bin));
				tangentsSpan[index] = new Vector4D<float>(tan, w);
			}

			if (info.Color > -1) {
				colorsSpan[index] = VertexHelper.UnpackColor(MemoryMarshal.Read<Vector4D<byte>>(vertex[info.Color..]));
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

	public static bool ConvertLooseGeometry(string path, IConversionOptions flags, IMemoryBuffer<byte> data) {
		using var geometry = new Geometry(data);

		var gltfPath = Path.ChangeExtension(path, ".gltf");
		var bufferPath = Path.ChangeExtension(path, ".glbin");
		using Stream bufferStream = flags.Dry ? new MemoryStream() : new FileStream(bufferPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

		var gltf = new GL.Root();

		var root = gltf.CreateNode().Node;
		root.Name = Path.GetFileNameWithoutExtension(path);

		var buffers = BuildGeometryBuffers(gltf, bufferStream, geometry, root.Name);
		var existingPrimitives = new PrimitiveCache();

		var indexBuffers = geometry.SharedIndexBuffers
								   .Where(x => x.Key.Text.EndsWith(".indices"))
								   .ToDictionary(x => x.Key.Text[..^8], x => x.Value);

		foreach (var vertexBuffer in geometry.SharedVertexBuffers.Values.Where(x => x.Name.Text.EndsWith(".vertices"))) {
			var name = vertexBuffer.Name.Text[..^9];

			if (!indexBuffers.TryGetValue(name, out var indexBuffer)) {
				continue;
			}

			var (meshNode, _) = root.CreateNode(gltf);

			(var mesh, meshNode.Mesh) = gltf.CreateMesh();
			mesh.Name = meshNode.Name = name;

			CreatePrimitive(gltf, mesh, buffers, existingPrimitives, vertexBuffer, indexBuffer, geometry);
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

	private static void CreateArmor(GL.Root gltf, GL.Node node, Stream stream, GeometryArmor armor, Dictionary<int, int> thicknessMaterial) {
		var (meshNode, _) = node.CreateNode(gltf);
		(var mesh, meshNode.Mesh) = gltf.CreateMesh();
		mesh.Name = meshNode.Name = armor.Name;

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

	private static GL.Primitive CreatePrimitive(GL.Root gltf, GL.Mesh mesh,
		GeometryCache buffers, PrimitiveCache existingPrimitives, GeometryName vertexBuffer, GeometryName indexBuffer, Geometry geometry) {
		var mergedVbo = buffers[(true, vertexBuffer.BufferIndex)];
		var mergedIbo = buffers[(false, indexBuffer.BufferIndex)];

		var primitive = new GL.Primitive {
			Mode = GL.PrimitiveMode.Triangles,
		};

		if (!existingPrimitives.TryGetValue((vertexBuffer, indexBuffer), out var pair)) {
			var iboStride = geometry.MergedIndexBuffers[indexBuffer.BufferIndex].Stride;

			primitive.Indices = gltf.CreateAccessor(mergedIbo["INDEX"], indexBuffer.BufferLength,
				indexBuffer.BufferOffset * iboStride, GL.AccessorType.SCALAR,
				iboStride == 2 ? GL.AccessorComponentType.UnsignedShort : GL.AccessorComponentType.UnsignedInt).Id;

			foreach (var (type, bufferIndex) in mergedVbo) {
				var (accessorType, accessorComponentType, stride) = type switch {
					"POSITION" => (GL.AccessorType.VEC3, GL.AccessorComponentType.Float, 12),
					"NORMAL" => (GL.AccessorType.VEC3, GL.AccessorComponentType.Float, 12),
					"TEXCOORD_0" => (GL.AccessorType.VEC2, GL.AccessorComponentType.Float, 8),
					"TEXCOORD_1" => (GL.AccessorType.VEC2, GL.AccessorComponentType.Float, 8),
					"TANGENT" => (GL.AccessorType.VEC4, GL.AccessorComponentType.Float, 16),
					"COLOR_0" => (GL.AccessorType.VEC4, GL.AccessorComponentType.Float, 16),
					_ => throw new NotImplementedException(),
				};

				var (_, accessorId) = gltf.CreateAccessor(bufferIndex, vertexBuffer.BufferLength,
					vertexBuffer.BufferOffset * stride, accessorType, accessorComponentType);

				primitive.Attributes[type] = accessorId;

				/*
					if (type == "POSITION") {
						accessor.Max = [double.MinValue, double.MinValue, double.MinValue];
						accessor.Min = [double.MaxValue, double.MaxValue, double.MaxValue];
						// todo: calculate bounds
					}
					*/
			}

			existingPrimitives[(vertexBuffer, indexBuffer)] = (primitive.Attributes, primitive.Indices.Value);
		} else {
			primitive.Indices = pair.Indices;
			primitive.Attributes = pair.Attributes;
		}

		mesh.Primitives.Add(primitive);
		return primitive;
	}

	public static VisualPrototype? FindVisualPrototype(ResourceManager manager, string path) {
		var prototype = manager.OpenPrototype(path);
		while (prototype is not null) {
			switch (prototype) {
				case VisualPrototype visualPrototype:
					return visualPrototype;
				case ModelPrototype modelPrototype:
					prototype = manager.OpenPrototype(modelPrototype.VisualResource.Hash);
					break;
				default:
					return null;
			}
		}

		return null;
	}

	public static void ConvertVisual(ResourceManager manager, string fileName, string path,
		string rootModelPath, Dictionary<string, string> hardPoints,
		IConversionOptions flags, ParamTypeInfo? info,
		string? subdir = null, string? parentName = null) {
		var builtVisual = FindVisualPrototype(manager, rootModelPath);
		if (builtVisual == null) {
			return;
		}

		var resolvedParts = ResolveShipParts(builtVisual, rootModelPath);
		var portPoints = new Dictionary<string, string>();
		foreach (var (hardpoint, (resolvedPart, portsPart)) in resolvedParts) {
			portPoints[hardpoint] = portsPart;
			hardPoints[hardpoint] = resolvedPart;
		}

		string modelPath;
		parentName ??= fileName;
		if (flags.InsertTypeInfo && info != null) {
			modelPath = Path.Combine(path,
				info.Type.TrimStart('/', '.').ToLowerInvariant(),
				info.Nation?.TrimStart('/', '.').ToLowerInvariant() ?? "unknown",
				info.Species?.TrimStart('/', '.').ToLowerInvariant() ?? "unknown", parentName);
		} else {
			modelPath = Path.Combine(path, parentName);
		}

		if (!string.IsNullOrEmpty(subdir)) {
			modelPath = Path.Combine(modelPath, subdir);
		}

		var texturesPath = Path.Combine(path, "textures");
		Directory.CreateDirectory(modelPath);
		Directory.CreateDirectory(texturesPath);

		var gltfPath = Path.Combine(modelPath, fileName + ".gltf");
		var bufferPath = Path.ChangeExtension(gltfPath, ".glbin");
		using Stream bufferStream = flags.Dry ? new MemoryStream() : new FileStream(bufferPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

		var gltf = new GL.Root();

		var root = gltf.CreateNode().Node;
		root.Name = fileName;

		var primCache = new Dictionary<ResourceId, PrimitiveCache>();
		var geoCache = new Dictionary<ResourceId, GeometryCache>();
		var materialCache = new Dictionary<ResourceId, int>();
		var textureCache = new Dictionary<ResourceId, int>();
		var context = new BuilderContext(
			flags, manager, bufferStream, modelPath, texturesPath,
			hardPoints, portPoints, primCache, geoCache, materialCache, textureCache);
		BuildModelPart(context, gltf, root, builtVisual);

		gltf.Buffers = [
			new GL.Buffer {
				Uri = Path.GetFileName(bufferPath),
				ByteLength = bufferStream.Length,
			},
		];

		if (flags.Dry) {
			return;
		}

		using var file = new FileStream(gltfPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		JsonSerializer.Serialize(file, gltf, GltfOptions);
		file.Write(Encoding.UTF8.GetBytes(Environment.NewLine));
	}

	private static void BuildModelPart(BuilderContext context, GL.Root gltf, GL.Node parent, string modelPath) {
		AkizukiLog.Information("Building part {Path}", modelPath);
		var builtVisual = FindVisualPrototype(context.Manager, modelPath);
		if (builtVisual == null) {
			return;
		}

		BuildModelPart(context, gltf, parent, builtVisual);
	}

	private static void BuildModelPart(BuilderContext context, GL.Root gltf, GL.Node parent, VisualPrototype visual) {
		var name = Path.GetFileNameWithoutExtension(visual.MergedGeometryPath.Path);
		var (node, rootId) = parent.CreateNode(gltf);
		node.Name = name;

		var nodeMap = new Dictionary<StringId, GL.Node>();
		var boneCount = visual.Skeleton.Names.Count;
		var isSkinned = visual.RenderSets.Values.Any(x => x is { IsSkinned: true, Nodes.Count: > 1 });
		if (visual.Skeleton.Names.Count > 0) {
			var worldMatrices = isSkinned ? stackalloc Matrix4x4[boneCount] : [];
			GL.Skin? skin = null;
			if (isSkinned) {
				(skin, node.Skin) = gltf.CreateSkin();
			}

			for (var nodeIndex = 0; nodeIndex < visual.Skeleton.Names.Count; nodeIndex++) {
				var nodeName = visual.Skeleton.Names[nodeIndex];
				var nodeParent = visual.Skeleton.ParentIds[nodeIndex];
				var nodeMatrix = visual.Skeleton.Matrices[nodeIndex];
				var parentNode = nodeParent == ushort.MaxValue ? node : nodeMap[visual.Skeleton.Names[nodeParent]];

				GL.Node skeletonNode;
				int skeletonId;
				var realName = nodeName.Text;
				if (nodeIndex == 0) {
					skeletonNode = node;
					skeletonId = rootId;
				} else {
					(skeletonNode, skeletonId) = parentNode.CreateNode(gltf);
					skeletonNode.Name = realName;
				}

				nodeMap[nodeName] = skeletonNode;

				if (context.HardPoints.TryGetValue(realName, out var hardPart)) {
					BuildModelPart(context, gltf, skeletonNode, hardPart);
				}

				if (context.PortPoints.TryGetValue(realName, out var portPart)) {
					BuildModelPart(context, gltf, skeletonNode, portPart);
				}

				if (LocateMiscObject(realName) is { } miscObject) {
					BuildModelPart(context, gltf, skeletonNode, miscObject);
				}

				if (isSkinned) {
					var parentMatrix = nodeParent == ushort.MaxValue ? Matrix4x4.Identity : worldMatrices[nodeParent];
					worldMatrices[nodeIndex] = nodeMatrix.ToSystem() * parentMatrix;
					skin!.Joints.Add(skeletonId);
				}

				var doubleMatrix = nodeMatrix.As<double>();
				var asSpan = new Span<Matrix4X4<double>>(ref doubleMatrix);
				skeletonNode.Matrix = MemoryMarshal.Cast<Matrix4X4<double>, double>(asSpan).ToArray().ToList();
			}

			if (isSkinned) {
				for (var index = 0; index < boneCount; ++index) {
					Matrix4x4.Invert(worldMatrices[index], out var inverseMatrix);
					worldMatrices[index] = inverseMatrix;
				}

				skin!.InverseBindMatrices = gltf.CreateAccessor(worldMatrices, context.BufferStream, GL.BufferViewTarget.ArrayBuffer, GL.AccessorType.MAT4, GL.AccessorComponentType.Float).Id;
			}
		}

		if (visual.LOD.Count == 0) {
			Debug.Assert(visual.RenderSets.Count == 0);
			return;
		}

		using var geometryData = context.Manager.OpenFile(visual.MergedGeometryPath);
		if (geometryData == null) {
			return;
		}

		using var geometry = new Geometry(geometryData);

		if (!context.GeometryCache.TryGetValue(visual.MergedGeometryPath, out var buffers)) {
			buffers = context.GeometryCache[visual.MergedGeometryPath] = BuildGeometryBuffers(gltf, context.BufferStream, geometry, name);
		}

		if (!context.PrimCache.TryGetValue(visual.MergedGeometryPath, out var primCache)) {
			primCache = context.PrimCache[visual.MergedGeometryPath] = new PrimitiveCache();
		}

		var firstLod = visual.LOD.MinBy(x => x.Extent)!.RenderSets;
		foreach (var renderSet in firstLod.Select(x => visual.RenderSets[x])) {
			var vertexBuffer = geometry.SharedVertexBuffers[renderSet.VerticesName];
			var vertexHasBones = geometry.MergedVertexBuffers[vertexBuffer.BufferIndex].FormatName.Contains("iii", StringComparison.Ordinal);
			var setIsSkinned = isSkinned && renderSet is { IsSkinned: true, Nodes.Count: > 1 } && vertexHasBones;

			var shouldUseRoot = setIsSkinned || renderSet.Nodes.Count == 0 || nodeMap.Count == 0;
			var primaryNode = shouldUseRoot ? node : nodeMap[renderSet.Nodes[0]];
			var indicesBuffer = geometry.SharedIndexBuffers[renderSet.IndicesName];
			var material = renderSet.MaterialResource;

			if (primaryNode.Mesh is not { } meshId) {
				var pair = gltf.CreateMesh();
				pair.Mesh.Name = node.Name + "_" + renderSet.Name.Text;
				meshId = pair.Id;
				primaryNode.Mesh = meshId;
			}

			var mesh = gltf.Meshes![meshId];
			var prim = CreatePrimitive(gltf, mesh, buffers, primCache, vertexBuffer, indicesBuffer, geometry);
			if (setIsSkinned) {
				BuildBoneMap(context, gltf, prim, vertexBuffer, geometry, renderSet, visual);
			}

			if (!context.MaterialCache.TryGetValue(material, out var materialId)) {
				materialId = context.MaterialCache[material] = CreateMaterial(context, gltf, material);
			}

			if (materialId >= 0) {
				prim.Material = materialId;
			}
		}

		var thicknessMaterial = new Dictionary<int, int>();
		foreach (var armor in geometry.Armors) {
			CreateArmor(gltf, node, context.BufferStream, armor, thicknessMaterial);
		}
	}

	private static void BuildBoneMap(BuilderContext context, GL.Root gltf, GL.Primitive prim, GeometryName vertexBufferName, Geometry geometry,
		RenderSetPrototype renderSet, VisualPrototype visual) {
		if (prim.Attributes.ContainsKey("JOINTS_0")) {
			return;
		}

		var vertexBuffer = geometry.MergedVertexBuffers[vertexBufferName.BufferIndex];
		BuildSkinBuffer(gltf, context.BufferStream, vertexBuffer, vertexBufferName, prim.Attributes, renderSet, visual);
	}

	private static int CreateTexture(BuilderContext context, GL.Root gltf, ResourceId id, StringId slotName) {
		AkizukiLog.Information("Converting texture {Path}", id);
		if (context.TextureCache.TryGetValue(id, out var texId)) {
			return texId;
		}

		var texturePkgPath = id.Path;
		var isDDS = texturePkgPath.EndsWith(".dds");
		if (isDDS) {
			for (var hdIndex = 0; hdIndex < 2; ++hdIndex) {
				var hdPath = id.Path[..^1] + hdIndex.ToString("D");
				if (!context.Manager.HasFile(hdPath)) {
					continue;
				}

				texturePkgPath = hdPath;
				break;
			}
		}

		using var buffer = context.Manager.OpenFile(texturePkgPath);
		if (buffer == null) {
			return context.TextureCache[id] = -1;
		}

		var texturePath = Path.Combine(context.TexturesPath, $"{id.Hash:x16}-" + Path.GetFileName(texturePkgPath));
		if (context.Flags.SelectedFormat is not TextureFormat.None && isDDS) {
			if (ConvertTexture(texturePath, context.Flags, buffer, true, slotName == NormalMapId, slotName == MetalMapId) is not { } newTexturePath) {
				return context.TextureCache[id] = -1;
			}

			texturePath = Path.GetRelativePath(context.ModelPath, newTexturePath);
		} else if (!context.Flags.Dry) {
			using var stream = new FileStream(texturePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
			stream.Write(buffer.Span);
		}


		return context.TextureCache[id] = gltf.CreateTexture(texturePath, GL.WrapMode.Repeat, GL.WrapMode.Repeat, null, null).Id;
	}

	private static int CreateMaterial(BuilderContext context, GL.Root gltf, ResourceId material) {
		AkizukiLog.Information("Converting material {Path}", material);
		if (context.Manager.OpenPrototype(material) is not MaterialPrototype mfm) {
			return -1;
		}

		var materialAttributes = new CHRONOVOREMaterialAttributes {
			Scalars = [],
			Textures = [],
			Colors = [],
		};

		gltf.ExtensionsUsed ??= [];
		gltf.ExtensionsUsed.Add(CHRONOVOREMaterialAttributes.EXT_NAME);

		var (mat, matId) = gltf.CreateMaterial();

		mat.Name = Path.GetFileNameWithoutExtension(material.Path);

		foreach (var (name, value) in mfm.BoolValues) {
			materialAttributes.Scalars[name.Text] = value ? 1 : 0;
		}

		foreach (var (name, value) in mfm.IntValues) {
			materialAttributes.Scalars[name.Text] = value;
		}

		foreach (var (name, value) in mfm.UIntValues) {
			materialAttributes.Scalars[name.Text] = value;
		}

		foreach (var (name, value) in mfm.FloatValues) {
			materialAttributes.Scalars[name.Text] = value;
		}

		foreach (var (name, value) in mfm.TextureValues) {
			var textureId = CreateTexture(context, gltf, value, name);
			if (textureId == -1) {
				continue;
			}

			var texInfo = materialAttributes.Textures[name.Text] = new GL.TextureInfo {
				Index = textureId,
			};

			switch (name.Text) {
				case "diffuseMap":
					mat.PBR ??= new GL.PBRMaterial();
					mat.PBR.BaseColorTexture = texInfo;
					break;
				case "metallicGlossMap":
					mat.PBR ??= new GL.PBRMaterial();
					mat.PBR.MetallicRoughnessTexture = texInfo;
					break;
				case "ambientOcclusionMap":
					mat.OcclusionTexture = new GL.OcclusionTextureInfo {
						Index = texInfo.Index,
					};
					break;
				case "normalMap":
					mat.NormalTexture = new GL.NormalTextureInfo {
						Index = texInfo.Index,
					};
					break;
			}
		}

		foreach (var (name, value) in mfm.Vector2Values) {
			materialAttributes.Colors[name.Text] = [value.X, value.Y, 0.0, 1.0];
		}

		foreach (var (name, value) in mfm.Vector3Values) {
			materialAttributes.Colors[name.Text] = [value.X, value.Y, value.Z, 1.0];
		}

		foreach (var (name, value) in mfm.Vector4Values) {
			materialAttributes.Colors[name.Text] = [value.X, value.Y, value.Z, value.W];
		}

		return matId;
	}

	private record BuilderContext(
		IConversionOptions Flags,
		ResourceManager Manager,
		Stream BufferStream,
		string ModelPath,
		string TexturesPath,
		Dictionary<string, string> HardPoints,
		Dictionary<string, string> PortPoints,
		Dictionary<ResourceId, PrimitiveCache> PrimCache,
		Dictionary<ResourceId, GeometryCache> GeometryCache,
		Dictionary<ResourceId, int> MaterialCache,
		Dictionary<ResourceId, int> TextureCache);
}
