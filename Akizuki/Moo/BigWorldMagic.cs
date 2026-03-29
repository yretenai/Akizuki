// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Pluto.SourceGen.MagicGenerator;

namespace Akizuki.Moo;

[GenerateMagic]
public static partial class BigWorldMagic {
	[Magic("PFSI")]
	public static partial uint PackageIndex { get; }

	[Magic("BWDB")]
	public static partial uint AssetDatabase { get; }
}
