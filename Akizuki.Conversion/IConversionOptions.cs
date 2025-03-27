// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Triton.Encoder;

namespace Akizuki.Conversion;

public interface IConversionOptions {
	public bool Dry { get; }
	public TextureFormat Format { get; }
	public TextureFormat SelectedFormat { get; }
	public IEncoder? FormatEncoder { get; }
}
