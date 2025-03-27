// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;

namespace Akizuki;

internal static class MethodConstants {
	public const MethodImplOptions Inline = MethodImplOptions.AggressiveInlining;
#if DEBUG
	public const MethodImplOptions Optimize = 0;
#else
	public const MethodImplOptions Optimize = MethodImplOptions.AggressiveOptimization;
#endif
	public const MethodImplOptions InlineAndOptimize = Inline | Optimize;
}
