// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data;

// MurmurHash3_32
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct StringId(uint Hash) {
	public string Text => ResourceManager.Instance?.Database?.GetString(Hash) ?? Hash.ToString("x8");
	public override string ToString() => Text;
	public override int GetHashCode() => Hash.GetHashCode();
	public bool Equals(StringId? other) => other?.Hash == Hash;
}
