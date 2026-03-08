// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics.CodeAnalysis;

namespace Akizuki;

public static class Extensions {
	extension(GameDataObject obj) {
		[return: NotNullIfNotNull("defaultValue")]
		public T GetValueOrDefault<T>(string key, T? defaultValue = default) {
			if (!obj.TryGetValue(key, out var value) || value is not T tValue) {
				return defaultValue!;
			}

			return tValue;
		}

		public bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T result) {
			if (!obj.TryGetValue(key, out var value) || value is not T tValue) {
				result = default;
				return false;
			}

			result = tValue;
			return true;
		}

		public T GetValue<T>(string key) {
			if (!obj.TryGetValue(key, out var value) || value is not T tValue) {
				throw new KeyNotFoundException();
			}

			return tValue;
		}

		public T GetParamOrDefault<T>(string key, T? defaultValue = default) where T : new() {
			if (!obj.TryGetValue(key, out var value) || value is not GameDataObject tValue) {
				return defaultValue ?? new T();
			}

			return (T) Activator.CreateInstance(typeof(T), tValue)!;
		}

		public T GetParam<T>(string key) where T : new() {
			if (!obj.TryGetValue(key, out var value) || value is not GameDataObject tValue) {
				throw new KeyNotFoundException();
			}

			return (T) Activator.CreateInstance(typeof(T), tValue)!;
		}
	}
}
