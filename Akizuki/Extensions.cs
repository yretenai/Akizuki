// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics.CodeAnalysis;
using DragonLib.IO;

namespace Akizuki;

public static class Extensions {
	public static IMemoryBuffer<byte> ToRented(this Stream stream) {
		var size = (int) (stream.Length - stream.Position);
		var memory = new MemoryBuffer<byte>(size);
		stream.ReadExactly(memory.Span);
		return memory;
	}

	[return: NotNullIfNotNull("defaultValue")]
	public static T GetValueOrDefault<T>(this GameDataObject obj, string key, T? defaultValue = default) {
		if (!obj.TryGetValue(key, out var value) || value is not T tValue) {
			return defaultValue!;
		}

		return tValue;
	}

	public static bool TryGetValue<T>(this GameDataObject obj, string key, [MaybeNullWhen(false)] out T result) {
		if (!obj.TryGetValue(key, out var value) || value is not T tValue) {
			result = default;
			return false;
		}

		result = tValue;
		return true;
	}

	public static T GetValue<T>(this GameDataObject obj, string key) {
		if (!obj.TryGetValue(key, out var value) || value is not T tValue) {
			throw new KeyNotFoundException();
		}

		return tValue;
	}

	public static T GetParamOrDefault<T>(this GameDataObject obj, string key, T? defaultValue = default) where T : new() {
		if (!obj.TryGetValue(key, out var value) || value is not GameDataObject tValue) {
			return defaultValue ?? new T();
		}

		return (T) Activator.CreateInstance(typeof(T), tValue)!;
	}

	public static T GetParam<T>(this GameDataObject obj, string key) where T : new() {
		if (!obj.TryGetValue(key, out var value) || value is not GameDataObject tValue) {
			throw new KeyNotFoundException();
		}

		return (T) Activator.CreateInstance(typeof(T), tValue)!;
	}
}
