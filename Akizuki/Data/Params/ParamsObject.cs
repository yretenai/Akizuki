// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace Akizuki.Data.Params;

public class ParamObject {
	public ParamObject() { }

	public ParamObject(GameDataObject data) => ParamTypeInfo = data.GetParam<ParamTypeInfo>("typeinfo");

	public ParamTypeInfo ParamTypeInfo { get; set; } = new();
}
