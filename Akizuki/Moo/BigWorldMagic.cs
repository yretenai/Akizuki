// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using DragonLib.SourceGen.MagicGenerator;

namespace Akizuki.Moo;

[GenerateMagic]
public sealed partial class BigWorldMagic {
	[Magic("PFSI")]
	public partial uint PackageIndex { get; }

	[Magic("BWDB")]
	public partial uint AssetDatabase { get; }
}
