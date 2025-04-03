// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Akizuki.Conversion.Utility;
using Akizuki.Data.Params;
using Akizuki.Data.Tables;
using Akizuki.Graphics;
using Akizuki.Structs.Data;
using Akizuki.Structs.Data.Camouflage;
using Akizuki.Structs.Graphics;
using Akizuki.Structs.Graphics.VertexFormat;
using DragonLib.IO;
using GLTF.Scaffold.Extensions;
using Silk.NET.Maths;
using GL = GLTF.Scaffold;
using GeometryCache = System.Collections.Generic.Dictionary<(bool IsVertexBuffer, int GeometryBufferId), System.Collections.Generic.Dictionary<string, int>>;
using PrimitiveCache = System.Collections.Generic.Dictionary<(Akizuki.Structs.Graphics.GeometryName Vertex, Akizuki.Structs.Graphics.GeometryName Index), (System.Collections.Generic.Dictionary<string, int> Attributes, int Indices)>;

namespace Akizuki.Conversion;

public static class GeometryConverter {
	static GeometryConverter() {
		ReadBlockList(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "MiscBlockList.txt"), MiscBlockList);
	}

	// I have no idea how the game resolves misc objects
	// At the moment Akizuki tries to resolve the misc object based on heuristics, but sometimes invalid misc objects are found.
	// Reads ./Resources/MiscBlockList.txt
	public static HashSet<string> MiscBlockList { get; } = [];

	private static void ReadBlockList(string path, HashSet<string> blockList) {
		if (!File.Exists(path)) {
			return;
		}

		using var reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
		while (reader.ReadLine() is { } line) {
			var semiColonIndex = line.IndexOf(';', StringComparison.Ordinal);
			switch (semiColonIndex) {
				case 0: continue;
				case > -1: line = line[..semiColonIndex]; break;
			}

			line = line.Trim();
			if (line.Length > 0) {
				blockList.Add(line);
			}
		}
	}


	[MethodImpl(MethodConstants.Optimize)]
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

	[MethodImpl(MethodConstants.Optimize)]
	public static string? LocateMiscObject(string id) {
		if (!id.StartsWith("MP_") || MiscBlockList.Contains(id)) {
			return null;
		}

		id = id[3..];

		if (id.Length < 5 || id[1] != 'M' || MiscBlockList.Contains(id)) {
			return null;
		}

		if (id[2] - '0' >= 5) {
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

	[MethodImpl(MethodConstants.Optimize)]
	public static GeometryCache BuildGeometryBuffers(IConversionOptions options, GL.Root gltf, Stream stream, Geometry geometry, string name, List<Matrix4X4<float>> matrices) {
		var mapping = new Dictionary<(bool, int), Dictionary<string, int>>();

		for (var index = 0; index < geometry.MergedIndexBuffers.Count; index++) {
			var indexBuffer = geometry.MergedIndexBuffers[index];
			var id = gltf.CreateBufferView(indexBuffer.Buffer.Span, stream, -1, GL.BufferViewTarget.ElementArrayBuffer).Id;

			mapping[(false, index)] = new Dictionary<string, int> {
				["INDEX"] = id,
			};
		}

		var shouldFlipNormals = matrices.Any(matrix => matrix.GetDeterminant() < 0);

		for (var index = 0; index < geometry.MergedVertexBuffers.Count; index++) {
			var vertexBuffer = geometry.MergedVertexBuffers[index];
			var id = BuildVertexBuffer(options, gltf, stream, vertexBuffer, name, vertexBuffer.Header.IsSkinned && shouldFlipNormals);
			if (id.Count == 0) {
				continue;
			}

			mapping[(true, index)] = id;
		}

		return mapping;
	}

	[MethodImpl(MethodConstants.Optimize)]
	public static void BuildSkinBuffer(GL.Root gltf, Stream stream, GeometryVertexBuffer vertexBuffer, GeometryName vertexBufferName, Dictionary<string, int> attributes, RenderSetPrototype renderSet, VisualPrototype visual) {
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

	[MethodImpl(MethodConstants.Optimize)]
	public static Dictionary<string, int> BuildVertexBuffer(IConversionOptions options, GL.Root gltf, Stream stream, GeometryVertexBuffer vertexBuffer, string name, bool flipNormals) {
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

		for (var index = 0; index < vertexBuffer.VertexCount; index += 1) {
			var vertex = buffer.Slice(index * vertexBuffer.Stride, vertexBuffer.Stride);

			positionsSpan[index] = MemoryMarshal.Read<Vector3D<float>>(vertex[info.Position..]);
			var nor = VertexHelper.UnpackNormal(MemoryMarshal.Read<Vector4D<sbyte>>(vertex[info.Normal..]));
			if (flipNormals) {
				nor *= -1;
			}
			normals[index] = nor;
			uv1Span[index] = VertexHelper.UnpackUV(MemoryMarshal.Read<Vector2D<Half>>(vertex[info.UV1..]));

		#pragma warning disable CA1508
			if (info.UV2 > -1) {
				uv2Span[index] = VertexHelper.UnpackUV(MemoryMarshal.Read<Vector2D<Half>>(vertex[info.UV2..]));
			}

			if (info.Tangent > -1) {
				var tan = VertexHelper.UnpackNormal(MemoryMarshal.Read<Vector4D<sbyte>>(vertex[info.Tangent..]));
				var bin = VertexHelper.UnpackNormal(MemoryMarshal.Read<Vector4D<sbyte>>(vertex[info.Binormal..]));
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

		if (colorsSpan.Length > 0 && !options.BlenderSafe) {
			result["COLOR_0"] = gltf.CreateBufferView(MemoryMarshal.AsBytes(colorsSpan), stream, Unsafe.SizeOf<Vector4D<float>>(), GL.BufferViewTarget.ArrayBuffer).Id;
		}

		return result;
	}

	[MethodImpl(MethodConstants.Optimize)]
	public static bool ConvertLooseGeometry(string path, IConversionOptions flags, IMemoryBuffer<byte> data) {
		using var geometry = new Geometry(data);

		var gltfPath = Path.ChangeExtension(path, ".gltf");
		var bufferPath = Path.ChangeExtension(path, ".glbin");
		using Stream bufferStream = flags.Dry ? new MemoryStream() : new FileStream(bufferPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
		bufferStream.SetLength(0);

		var gltf = new GL.Root();

		var root = gltf.CreateNode(Path.GetFileNameWithoutExtension(path)).Node;

		var buffers = BuildGeometryBuffers(flags, gltf, bufferStream, geometry, root.Name!, []);
		var existingPrimitives = new PrimitiveCache();

		var indexBuffers = geometry.SharedIndexBuffers
								   .Where(x => x.Key.Text.EndsWith(".indices"))
								   .ToDictionary(x => x.Key.Text[..^8], x => x.Value);

		foreach (var vertexBuffer in geometry.SharedVertexBuffers.Values.Where(x => x.Name.Text.EndsWith(".vertices"))) {
			var name = vertexBuffer.Name.Text[..^9];

			if (!indexBuffers.TryGetValue(name, out var indexBuffer)) {
				continue;
			}

			var (meshNode, _) = root.CreateNode(gltf, name);
			(var mesh, meshNode.Mesh) = gltf.CreateMesh(name);

			CreatePrimitive(gltf, mesh, buffers, existingPrimitives, vertexBuffer, indexBuffer, geometry);
		}

		if (flags.ConvertArmor) {
			var thicknessMaterial = new Dictionary<int, int>();
			foreach (var armor in geometry.Armors) {
				ArmorConverter.CreateArmor(gltf, root, bufferStream, armor, thicknessMaterial);
			}
		}

		gltf.Buffers = [
			new GL.Buffer {
				Name = null!,
				Uri = Path.GetFileName(bufferPath),
				ByteLength = bufferStream.Length,
			},
		];

		if (flags.Dry) {
			return true;
		}

		using var file = new FileStream(gltfPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		file.SetLength(0);
		JsonSerializer.Serialize(file, gltf, JsonOptions.GltfOptions);
		file.Write(Encoding.UTF8.GetBytes(Environment.NewLine));
		return true;
	}

	[MethodImpl(MethodConstants.Optimize)]
	public static GL.Primitive CreatePrimitive(GL.Root gltf, GL.Mesh mesh,
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

	[MethodImpl(MethodConstants.Optimize)]
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

	[MethodImpl(MethodConstants.Optimize)]
	public static void ConvertVisual(ResourceManager manager, string fileName, string path,
		string rootModelPath, Dictionary<string, HashSet<string>> hardPoints,
		IConversionOptions flags, ParamTypeInfo? info,
		CamouflageContext? camouflage, string? subdir = null, string? parentName = null) {
		var builtVisual = FindVisualPrototype(manager, rootModelPath);
		if (builtVisual == null) {
			return;
		}

		var resolvedParts = ResolveShipParts(builtVisual, rootModelPath);
		var portPoints = new Dictionary<string, string>();
		foreach (var (hardpoint, (resolvedPart, portsPart)) in resolvedParts) {
			portPoints[hardpoint] = portsPart;
			if (!hardPoints.TryGetValue(hardpoint, out var hardpoints)) {
				hardpoints = hardPoints[hardpoint] = [];
			}

			hardpoints.Add(resolvedPart);
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
		bufferStream.SetLength(0);

		var gltf = new GL.Root();

		var root = gltf.CreateNode(fileName).Node;

		var context = new ModelBuilderContext(flags, manager, bufferStream, modelPath, texturesPath, hardPoints, portPoints);
		BuildModelPart(context, gltf, root, builtVisual, camouflage);

		gltf.Buffers = [
			new GL.Buffer {
				Name = null!,
				Uri = Path.GetFileName(bufferPath),
				ByteLength = bufferStream.Length,
			},
		];

		if (flags.Dry) {
			return;
		}

		using var file = new FileStream(gltfPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		file.SetLength(0);
		JsonSerializer.Serialize(file, gltf, JsonOptions.GltfOptions);
		file.Write(Encoding.UTF8.GetBytes(Environment.NewLine));
	}

	[MethodImpl(MethodConstants.Optimize)]
	public static void BuildModelPart(ModelBuilderContext context, GL.Root gltf, GL.Node parent, string modelPath, CamouflageContext? camouflage) {
		AkizukiLog.Information("Building part {Path}", modelPath);
		var builtVisual = FindVisualPrototype(context.Manager, modelPath);
		if (builtVisual == null) {
			return;
		}

		BuildModelPart(context, gltf, parent, builtVisual, camouflage);
	}

	[MethodImpl(MethodConstants.Optimize)]
	public static void BuildModelPart(ModelBuilderContext context, GL.Root gltf, GL.Node parent, VisualPrototype visual, CamouflageContext? camouflage) {
		var name = Path.GetFileNameWithoutExtension(visual.MergedGeometryPath.Path);
		var (node, rootId) = parent.CreateNode(gltf, name);

		var nodeMap = new Dictionary<StringId, GL.Node>();
		var boneCount = visual.Skeleton.Names.Count;
		var isSkinned = visual.RenderSets.Values.Any(x => x is { IsSkinned: true, Nodes.Count: > 1 });

		if (visual.Skeleton.Names.Count > 0) {
			var worldMatrices = isSkinned ? stackalloc Matrix4X4<float>[boneCount] : [];
			GL.Skin? skin = null;
			if (isSkinned) {
				(skin, node.Skin) = gltf.CreateSkin(name);
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
					(skeletonNode, skeletonId) = parentNode.CreateNode(gltf, realName);
				}

				nodeMap[nodeName] = skeletonNode;

				if (context.HardPoints.TryGetValue(realName, out var hardPart)) {
					foreach (var part in hardPart) {
						BuildModelPart(context, gltf, skeletonNode, part, camouflage);
					}
				}

				if (context.PortPoints.TryGetValue(realName, out var portPart)) {
					BuildModelPart(context, gltf, skeletonNode, portPart, camouflage);
				}

				if (LocateMiscObject(realName) is { } miscObject) {
					BuildModelPart(context, gltf, skeletonNode, miscObject, camouflage);
				}

				if (isSkinned) {
					var worldMatrix = nodeMatrix;
					if (nodeParent != ushort.MaxValue) {
						worldMatrix *= worldMatrices[nodeParent];
					}

					// BendBones have inverted scale??
					if (worldMatrix.GetDeterminant() < 0 && nodeName.Text.Contains("BlendBone", StringComparison.OrdinalIgnoreCase)) {
						worldMatrix.M11 = Math.Abs(worldMatrix.M11);
						worldMatrix.M22 = Math.Abs(worldMatrix.M22);
						worldMatrix.M33 = Math.Abs(worldMatrix.M33);
					}

					worldMatrices[nodeIndex] = worldMatrix;

					skin!.Joints.Add(skeletonId);
				}

				var doubleMatrix = nodeMatrix.As<double>();
				var asSpan = new Span<Matrix4X4<double>>(ref doubleMatrix);
				skeletonNode.Matrix = MemoryMarshal.Cast<Matrix4X4<double>, double>(asSpan).ToArray().ToList();
			}

			if (isSkinned) {
				Span<Matrix4X4<float>> inverseMatrices = stackalloc Matrix4X4<float>[boneCount];
				for (var index = 0; index < boneCount; ++index) {
					Matrix4X4.Invert(worldMatrices[index], out inverseMatrices[index]);
				}

				skin!.InverseBindMatrices = gltf.CreateAccessor(inverseMatrices, context.BufferStream, GL.BufferViewTarget.ArrayBuffer, GL.AccessorType.MAT4, GL.AccessorComponentType.Float).Id;
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
			buffers = context.GeometryCache[visual.MergedGeometryPath] = BuildGeometryBuffers(context.Flags, gltf, context.BufferStream, geometry, name, visual.Skeleton.Matrices);
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
				var pair = gltf.CreateMesh(node.Name + "_" + renderSet.Name.Text);
				meshId = pair.Id;
				primaryNode.Mesh = meshId;
			}

			var mesh = gltf.Meshes![meshId];
			var prim = CreatePrimitive(gltf, mesh, buffers, primCache, vertexBuffer, indicesBuffer, geometry);
			if (setIsSkinned) {
				BuildBoneMap(context, gltf, prim, vertexBuffer, geometry, renderSet, visual);
			}

			if (!context.MaterialCache.TryGetValue(material, out var materialId)) {
				materialId = context.MaterialCache[material] = CreateMaterial(context, gltf, material, camouflage);
			}

			if (materialId >= 0) {
				prim.Material = materialId;
			}
		}

		if (context.Flags.ConvertArmor) {
			foreach (var armor in geometry.Armors) {
				ArmorConverter.CreateArmor(gltf, node, context.BufferStream, armor, context.ThicknessMaterialCache);
			}
		}
	}

	[MethodImpl(MethodConstants.Optimize)]
	public static void BuildBoneMap(ModelBuilderContext context, GL.Root gltf, GL.Primitive prim, GeometryName vertexBufferName, Geometry geometry,
		RenderSetPrototype renderSet, VisualPrototype visual) {
		if (prim.Attributes.ContainsKey("JOINTS_0")) {
			return;
		}

		var vertexBuffer = geometry.MergedVertexBuffers[vertexBufferName.BufferIndex];
		BuildSkinBuffer(gltf, context.BufferStream, vertexBuffer, vertexBufferName, prim.Attributes, renderSet, visual);
	}

	[MethodImpl(MethodConstants.Optimize)]
	public static int CreateMaterial(ModelBuilderContext context, GL.Root gltf, ResourceId material, CamouflageContext? camouflage) {
		AkizukiLog.Information("Converting material {Path}", material);
		if (context.Manager.OpenPrototype(material) is not MaterialPrototype mfm) {
			return -1;
		}

		var materialAttributes = new CHRONOVOREMaterialAttributes {
			Scalars = [],
			Textures = [],
			Colors = [],
			Workflow = Path.GetFileNameWithoutExtension(mfm.FxPath.Path),
		};

		if (materialAttributes.Workflow.EndsWith("_skinned", StringComparison.Ordinal)) {
			materialAttributes.Workflow = materialAttributes.Workflow[..^8];
		}

		materialAttributes.Workflow = materialAttributes.Workflow switch {
			"ship_material" => "PBS_ship",
			"wire_material" => "PBS_ship",
			"PBS_wire" => "PBS_ship",
			"grid_alpha" => "PBS_grid",
			"glass_material" => "PBS_glass",
			_ => materialAttributes.Workflow,
		};

		gltf.ExtensionsUsed ??= [];
		gltf.ExtensionsUsed.Add(CHRONOVOREMaterialAttributes.EXT_NAME);

		var (mat, matId) = gltf.CreateMaterial(Path.GetFileNameWithoutExtension(material.Path));
		mat.DoubleSided = true;

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
			var textureId = TextureConverter.CreateTexture(context, gltf, value, name);
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

		if (camouflage is { Camouflage.Tiled: false }) {
			BuildCamouflage(context, gltf, mat.Name!, materialAttributes, camouflage);
		}

		mat.Extensions ??= [];
		mat.Extensions[CHRONOVOREMaterialAttributes.EXT_NAME] = JsonValue.Create(materialAttributes, JsonOptions.GltfNodeOptions)!;

		return matId;
	}

	private static void BuildCamouflage(ModelBuilderContext context, GL.Root gltf, string name, CHRONOVOREMaterialAttributes materialAttributes, CamouflageContext camouflage) {
		var part = camouflage.Part;
		if (name.Length > 2 && camouflage.Camouflage.Tiled == false) {
			if (part != CamouflagePart.Plane) {
				part = name[1] switch {
					'G' => CamouflagePart.Gun,
					'S' when name.Contains("_Bulge", StringComparison.Ordinal) => CamouflagePart.Bulge,
					'S' when name.Contains("_DeckHouse", StringComparison.Ordinal) => CamouflagePart.DeckHouse,
					'S' => CamouflagePart.Hull,
					'A' => CamouflagePart.Float,
					'D' => CamouflagePart.Director,
					'M' => CamouflagePart.Misc,
					_ when name.Contains("_wire", StringComparison.Ordinal) => CamouflagePart.Wire,
					_ => part,
				};
			} else {
				if (name[1] != 'A') {
					return;
				}
			}
		} else {
			return;
		}

		if (!camouflage.Camouflage.Textures.TryGetValue(part, out var texture)) {
			return;
		}

		materialAttributes.Workflow += "_camo";

		if (!camouflage.Camouflage.UV.TryGetValue(part, out var uv)) {
			uv = new Vector2D<float>(1, 1);
		}

		var textureId = TextureConverter.CreateTexture(context, gltf, context.Manager.FindFile(texture.Path), new StringId(0), uv);
		if (textureId == -1) {
			return;
		}

		materialAttributes.Textures ??= [];
		materialAttributes.Textures["camouflageMap"] = new GL.TextureInfo {
			Index = textureId,
		};

		materialAttributes.Colors ??= [];
		materialAttributes.Colors["camouflageColor0"] = [camouflage.ColorScheme.Color0.X, camouflage.ColorScheme.Color0.Y, camouflage.ColorScheme.Color0.Z, camouflage.ColorScheme.Color0.W];
		materialAttributes.Colors["camouflageColor1"] = [camouflage.ColorScheme.Color1.X, camouflage.ColorScheme.Color1.Y, camouflage.ColorScheme.Color1.Z, camouflage.ColorScheme.Color1.W];
		materialAttributes.Colors["camouflageColor2"] = [camouflage.ColorScheme.Color2.X, camouflage.ColorScheme.Color2.Y, camouflage.ColorScheme.Color2.Z, camouflage.ColorScheme.Color2.W];
		materialAttributes.Colors["camouflageColor3"] = [camouflage.ColorScheme.Color3.X, camouflage.ColorScheme.Color3.Y, camouflage.ColorScheme.Color3.Z, camouflage.ColorScheme.Color3.W];

		if (!camouflage.Camouflage.MGNTextures.TryGetValue(part, out texture)) {
			return;
		}

		textureId = TextureConverter.CreateTexture(context, gltf, context.Manager.FindFile(texture.Path), new StringId(0), uv);
		if (textureId == -1) {
			return;
		}

		materialAttributes.Textures["camouflageMgnMap"] = new GL.TextureInfo {
			Index = textureId,
		};

		if (texture.Influence is { } influence) {
			materialAttributes.Colors["camouflageMgnInfluence"] = [influence.X, influence.Y, influence.Z, influence.W];
		}
	}
}
