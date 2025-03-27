namespace Akizuki.Unpack.Conversion;

internal static class ColorTheory {
	internal static double[] ArmorStart = [68 / 255.0, 206 / 255.0, 27 / 255.0, 1.0];
	internal static double[] ArmorEnd = [229 / 255.0, 31 / 255.0, 31 / 255.0, 1.0];

	internal static List<double> LerpColor(double[] a, double[] b, double t) => [
		t <= 0.0 ? a[0] : t >= 1.0 ? b[0] : a[0] + (b[0] - a[0]) * t,
		t <= 0.0 ? a[1] : t >= 1.0 ? b[1] : a[1] + (b[1] - a[1]) * t,
		t <= 0.0 ? a[2] : t >= 1.0 ? b[2] : a[2] + (b[2] - a[2]) * t,
		1.0,
	];
}
