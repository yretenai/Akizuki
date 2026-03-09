// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Moo;

[StructLayout(LayoutKind.Explicit, Size = 4)]
public readonly record struct BigWorldVersion([field: FieldOffset(3)] byte Major = 0,  [field: FieldOffset(2)] byte Minor = 0, [field: FieldOffset(1)] byte Patch = 0, [field: FieldOffset(0)] byte Revision = 0) : IComparable<BigWorldVersion> {
	public static bool operator >(BigWorldVersion left, BigWorldVersion right) => left.CompareTo(right) > 0;
	public static bool operator <(BigWorldVersion left, BigWorldVersion right) => !(left > right);
	public static bool operator >=(BigWorldVersion left, BigWorldVersion right) => left > right || left == right;
	public static bool operator <=(BigWorldVersion left, BigWorldVersion right) => left < right || left == right;

	public override string ToString() => $"{Major}.{Minor}.{Patch}({Revision})";

	public int CompareTo(BigWorldVersion other) {
		var majorComparison = Major.CompareTo(other.Major);
		if (majorComparison != 0) {
			return majorComparison;
		}

		var minorComparison = Minor.CompareTo(other.Minor);
		if (minorComparison != 0) {
			return minorComparison;
		}

		var patchComparison = Patch.CompareTo(other.Patch);
		if (patchComparison != 0) {
			return patchComparison;
		}

		return Revision.CompareTo(other.Revision);
	}
}
