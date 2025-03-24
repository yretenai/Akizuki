// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Akizuki.Exceptions;
using Akizuki.Structs.Data;
using DragonLib.Hash.Algorithms;
using DragonLib.IO;

namespace Akizuki.Data;

public class BigWorldDatabase {
	public BigWorldDatabase(IMemoryBuffer<byte> buffer, bool leaveOpen = false, bool validate = false) {
		using var deferred = new DeferredDisposable(() => {
			if (!leaveOpen) {
				buffer.Dispose();
			}
		});

		using var data = new MemoryReader(buffer);

		BigWorldHeader = data.Read<BWFileHeader>();

		if (!BigWorldHeader.IsHostEndian) {
			throw new NotSupportedException("Database Big Endian is not supported");
		}

		if (BigWorldHeader.Version != 257) {
			throw new NotSupportedException("Only Database Version 257 is supported");
		}

		if (BigWorldHeader.Magic != BWFileHeader.BWDBMagic) {
			throw new InvalidDataException("File is not recognised as a Big World Database");
		}

		if (BigWorldHeader.PointerSize is not 64) {
			throw new NotSupportedException($"BWDB Bit Size of {BigWorldHeader.PointerSize} not supported");
		}

		var baseRel = Unsafe.SizeOf<BWFileHeader>();

		if (validate) {
			var hash = MurmurHash3Algorithm.Hash32_32(data.Buffer.Span[baseRel..]);
			if (hash != BigWorldHeader.Hash) {
				throw new CorruptDataException("BWDB Checksum Mismatch");
			}

			AkizukiLog.Verbose("BWDB Passed Checksum Validation");
		}

		Header = data.Read<BWDBHeader>();

		var resourceBase = baseRel + Unsafe.SizeOf<BWDBDictionary>() + Unsafe.SizeOf<ulong>() * 2;
		var pathsBase = resourceBase + Unsafe.SizeOf<BWDBDictionary>();
		
	#region Strings
		{
			data.Offset = (int) (baseRel + Header.Strings.KeyPtr);
			var assetIdKeys = data.Read<BWDBDictionaryKey<uint>>((int) Header.Strings.Count);
			data.Offset = (int) (baseRel + Header.Strings.ValuePtr);
			var assetIdValues = data.Read<int>((int) Header.Strings.Count);
			var assetIdsBlobPtr = (int) (baseRel + Header.StringsPtr);

			Strings.EnsureCapacity((int) Header.Strings.Count);
			for (var index = 0; index < assetIdKeys.Length; index++) {
				var assetId = assetIdKeys[index];
				var assetValueOffset = assetIdsBlobPtr + assetIdValues[index];
				if ((assetId.BucketId & 0x80000000) == 0) {
					continue;
				}

				data.Offset = assetValueOffset;
				Strings[assetId.Key] = data.ReadString();
			}
		}
	#endregion

	#region Paths
		{
			var nameOffset = (int) (pathsBase + Header.PathsPtr);
			data.Offset = nameOffset;
			var fileNames = data.Read<BWDBFileName>((int) Header.PathsCount);
			var names = new Dictionary<ulong, (string, BWDBFileName)>((int) Header.PathsCount);

			var oneNameEntry = Unsafe.SizeOf<BWDBFileName>();

			foreach (var fileName in fileNames) {
				data.Offset = nameOffset + (int) fileName.NamePtr + 0x10;
				names[fileName.Id] = (data.ReadString((int) fileName.NameLength - 1), fileName);
				nameOffset += oneNameEntry;
			}

			Paths.EnsureCapacity((int) Header.PathsCount);
			foreach (var fileName in fileNames) {
				ResolvePath(fileName, names);
			}
		}
	#endregion

	#region PathToPrototype

		{
			data.Offset = (int) (resourceBase + Header.ResourcePrototypes.KeyPtr);
			var resourceKeys = data.Read<BWDBDictionaryKey<ulong>>((int) Header.ResourcePrototypes.Count);
			data.Offset = (int) (resourceBase + Header.ResourcePrototypes.ValuePtr);
			var resourceValues = data.Read<int>((int) Header.ResourcePrototypes.Count);

			ResourceToPrototype.EnsureCapacity((int) Header.ResourcePrototypes.Count);
			for (var index = 0; index < resourceKeys.Length; index++) {
				var resourceId = resourceKeys[index];
				var bucketId = resourceId.BucketId >> 32;
				if ((bucketId & 0x80000000) == 0) {
					continue;
				}

				if (resourceValues[index] < 0) {
					continue;
				}
				
				// todo: resolve this.
				// "data only" assets, such as .model, .visual are stored in this file and named via this.
				// no clue how it resolves.
				ResourceToPrototype[resourceId.Key] = new BWPrototypeRef(Paths[resourceId.Key], resourceValues[index]);
			}
		}
	#endregion
	}
	public BWFileHeader BigWorldHeader { get; }
	public BWDBHeader Header { get; }
	public Dictionary<uint, string> Strings { get; } = [];
	public Dictionary<ulong, BWPrototypeRef> ResourceToPrototype { get; } = [];

	public Dictionary<ulong, string> Paths { get; } = new() {
		[0] = "res",
	};


	private void ResolvePath(BWDBFileName fileName, Dictionary<ulong, (string Name, BWDBFileName FileName)> names) {
		if (Paths.ContainsKey(fileName.Id)) {
			return;
		}

		ResolvePath(names[fileName.Id].Name, fileName.Id, fileName.ParentId, names);
	}

	private string ResolvePath(string name, ulong id, ulong parentId, Dictionary<ulong, (string Name, BWDBFileName FileName)> names) {
		if (Paths.TryGetValue(parentId, out var parentPath)) {
			return Paths[id] = parentPath + "/" + name;
		}

		var (parentName, parentFile) = names[parentId];
		return Paths[id] = ResolvePath(parentName, parentId, parentFile.ParentId, names) + "/" + name;
	}
}
