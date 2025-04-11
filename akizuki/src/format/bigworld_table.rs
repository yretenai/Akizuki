// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use binrw::BinRead;

#[derive(BinRead, Debug, Clone, Copy, Ord, PartialOrd, PartialEq, Eq, Hash)]
#[br(repr = u8)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub enum ModelMiscType {
	Structural,
	Necessary,
	Optional,
	Redundant,
	Undefined,
}
