// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using Triton;
using Triton.Encoder;

namespace Akizuki.Conversion;

public interface IConversionOptions {
	public bool Dry { get; }
	public TextureFormat ImageFormat { get; }
	public TextureFormat SelectedFormat { get; }
	public IEncoder? FormatEncoder { get; }
	public CubemapStyle CubemapStyle { get; }
	public bool ConvertTextures { get; }
	public bool ConvertCubeMaps { get; }
	public bool InsertTypeInfo { get; }
}
