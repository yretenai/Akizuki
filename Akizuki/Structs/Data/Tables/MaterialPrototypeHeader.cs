// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data.Tables;

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 0x78)]
public record struct MaterialPrototypeHeader {
	public ushort PropertyCount { get; set; }
	public byte BoolValuesCount { get; set; }
	public byte IntValuesCount { get; set; }
	public byte UIntValuesCount { get; set; }
	public byte FloatValuesCount { get; set; }
	public byte TextureValuesCount { get; set; }
	public byte Vector2ValuesCount { get; set; }
	public byte Vector3ValuesCount { get; set; }
	public byte Vector4ValuesCount { get; set; }
	public byte MatrixValuesCount { get; set; }
	public long PropertyNameIdsPtr { get; set; }
	public long PropertyIdsPtr { get; set; }
	public long BoolValuesPtr { get; set; }
	public long IntValuesPtr { get; set; }
	public long UIntValuesPtr { get; set; }
	public long FloatValuesPtr { get; set; }
	public long TextureValuesPtr { get; set; }
	public long Vector2ValuesPtr { get; set; }
	public long Vector3ValuesPtr { get; set; }
	public long Vector4ValuesPtr { get; set; }
	public long MatrixValuesPtr { get; set; }
	public ResourceId FxPathId { get; set; }
	public uint CollisionFlags { get; set; }
	public int SortOrder { get; set; }
}
