// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;

namespace Akizuki.Conversion.Utility;

public static class ColorTheory {
	public static readonly List<double> Armor0To14 = HexToFloat(0x78ab9a);
	public static readonly List<double> Armor14To24 = HexToFloat(0x95a666);
	public static readonly List<double> Armor24To29 = HexToFloat(0x9fa157);
	public static readonly List<double> Armor29To33 = HexToFloat(0xb19446);
	public static readonly List<double> Armor33To75 = HexToFloat(0xaf813f);
	public static readonly List<double> Armor75To100 = HexToFloat(0xae6e3c);
	public static readonly List<double> Armor100To400 = HexToFloat(0xa45537);
	public static readonly List<double> Armor400To1000 = HexToFloat(0x893d33);

	private static List<double> HexToFloat(uint hex) => [
		((hex >> 16) & 0xFF) / 255.0,
		((hex >> 8) & 0xFF) / 255.0,
		(hex & 0xFF) / 255.0,
		1.0,
	];

	public static string ThicknessToName(int thickness) {
		return thickness switch {
			<= 14 => "0 to 14mm Armor",
			<= 24 => "14 to 24mm Armor",
			<= 29 => "24 to 29mm Armor",
			<= 33 => "29 to 33mm Armor",
			<= 75 => "33 to 75mm Armor",
			<= 100 => "75 to 100mm Armor",
			<= 400 => "100 to 400mm Armor",
			_ => "400 to 1000mm Armor",
		};
	}

	public static List<double> ThicknessToColor(int thickness) {
		return thickness switch {
			<= 14 => Armor0To14,
			<= 24 => Armor14To24,
			<= 29 => Armor24To29,
			<= 33 => Armor29To33,
			<= 75 => Armor33To75,
			<= 100 => Armor75To100,
			<= 400 => Armor100To400,
			_ => Armor400To1000,
		};
	}
}
