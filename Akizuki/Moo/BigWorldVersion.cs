// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Moo;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public record struct BigWorldVersion {
	public byte Revision { get; set; }
	public byte Patch { get; set; }
	public byte Minor { get; set; }
	public byte Major { get; set; }

	public static bool operator >(BigWorldVersion left, BigWorldVersion right) {
		if (left.Major > right.Major) {
			return true;
		}

		if (left.Minor > right.Minor) {
			return true;
		}

		if (left.Patch > right.Patch) {
			return true;
		}

		return left.Revision > right.Revision;
	}

	public static bool operator <(BigWorldVersion left, BigWorldVersion right) => !(left > right);
	public static bool operator >=(BigWorldVersion left, BigWorldVersion right) => left > right || left == right;
	public static bool operator <=(BigWorldVersion left, BigWorldVersion right) => left < right || left == right;

	public override string ToString() => $"{Major}.{Minor}.{Patch}({Revision})";
}
