// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct ResourceId(ulong Hash) {
	public string Path => ResourceManager.Instance?.Database?.GetPath(Hash) ?? Hash.ToString("x16");
	public override string ToString() => Path;
}
