// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace Akizuki.Data.Params;

public class ParamsObject {
	public ParamsObject() { }

	public ParamsObject(GameDataObject data) => TypeInfo = data.GetParam<TypeInfo>("typeinfo");

	public TypeInfo TypeInfo { get; set; } = new();
}
