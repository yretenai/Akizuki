// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::identifiers::{ResourceId, StringId};
use binrw::{BinRead, PosValue};

#[derive(BinRead, Debug, Clone, Copy, Ord, PartialOrd, PartialEq, Eq, Hash)]
#[br(repr = u8)]
pub enum ModelMiscType {
	Structural,
	Necessary,
	Optional,
	Redundant,
	Undefined,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct ModelPrototypeHeader {
	pub relative_position: PosValue<()>,

	pub visual_resource_id: ResourceId,
	pub misc_type: ModelMiscType,
	pub animation_count: u8,
	#[br(pad_after = 5)]
	pub dye_count: u8,
	pub animation_offset: u64,
	pub dye_offset: u64,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct DyePrototypeHeader {
	pub relative_position: PosValue<()>,

	pub matter_id: StringId,
	pub replaces_id: StringId,
	#[br(pad_after = 3)]
	pub tint_count: u32,
	pub tint_name_ids_offset: u64,
	pub tint_material_ids_offset: u64,
}
