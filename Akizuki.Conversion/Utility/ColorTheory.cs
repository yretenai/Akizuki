// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;

namespace Akizuki.Conversion.Utility;

public static class ColorTheory {
	public static readonly double[] ArmorStart = [68 / 255.0, 27 / 255.0, 206 / 255.0, 1.0];
	public static readonly double[] ArmorEnd = [229 / 255.0, 31 / 255.0, 31 / 255.0, 1.0];

	[MethodImpl(MethodConstants.InlineAndOptimize)]
	public static List<double> LerpColor(double[] a, double[] b, double t) => [
		t <= 0.0 ? a[0] : t >= 1.0 ? b[0] : a[0] + (b[0] - a[0]) * t,
		t <= 0.0 ? a[1] : t >= 1.0 ? b[1] : a[1] + (b[1] - a[1]) * t,
		t <= 0.0 ? a[2] : t >= 1.0 ? b[2] : a[2] + (b[2] - a[2]) * t,
		1.0,
	];
}
