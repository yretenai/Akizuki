// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::error::AkizukiResult;
use crate::format::bigworld_table::ModelMiscType;
use crate::identifiers::{ResourceId, StringId};
use crate::table::model::DyePrototypeVersion;
use akizuki_macro::BigWorldTable;

use binrw::{BinRead, PosValue};
use binrw::{BinReaderExt, VecArgs};

use std::collections::HashMap;
use std::io::SeekFrom::Start;
use std::io::{Cursor, Seek};

#[derive(BinRead, Debug)]
#[br()]
pub struct ModelPrototypeHeader14 {
	pub relative_position: PosValue<()>,

	pub visual_resource: ResourceId,
	pub misc_type: ModelMiscType,
	pub animation_count: u8,
	pub dye_count: u8,
	#[br(pad_before = 5)]
	pub animation_offset: u64,
	pub dye_offset: u64,

	pub end_position: PosValue<()>,
}

#[derive(BinRead, Debug)]
#[br()]
pub struct DyePrototypeHeader14 {
	pub relative_position: PosValue<()>,

	pub matter_id: StringId,
	pub replaces_id: StringId,
	pub tint_count: u32,
	#[br(pad_before = 4)]
	pub tint_name_ids_offset: u64,
	pub tint_material_ids_offset: u64,
}

#[derive(Debug, BigWorldTable)]
#[table(ModelPrototype, 0xd6b11569)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub struct ModelPrototype14 {
	pub visual_resource: ResourceId,
	pub misc_type: ModelMiscType,
	pub animations: Vec<ResourceId>,
	pub dyes: Vec<DyePrototypeVersion>,
}

#[derive(Debug)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub struct DyePrototype14 {
	pub matter: StringId,
	pub replaces: StringId,
	pub tints: HashMap<StringId, ResourceId>,
}

impl ModelPrototype14 {
	pub fn new(reader: &mut Cursor<Vec<u8>>) -> AkizukiResult<Self> {
		let header = reader.read_ne::<ModelPrototypeHeader14>()?;

		reader.seek(Start(header.relative_position.pos + header.animation_offset))?;
		let animations = Vec::<ResourceId>::read_ne_args(
			reader,
			VecArgs {
				count: header.animation_count as usize,
				inner: <_>::default(),
			},
		)?;

		reader.seek(Start(header.relative_position.pos + header.dye_offset))?;
		let dye_headers = Vec::<DyePrototypeHeader14>::read_ne_args(
			reader,
			VecArgs {
				count: header.dye_count as usize,
				inner: <_>::default(),
			},
		)?;

		let mut dyes = Vec::<DyePrototypeVersion>::with_capacity(header.dye_count as usize);
		for dye_header in dye_headers {
			dyes.push(DyePrototypeVersion::V14(DyePrototype14::new(reader, dye_header)?));
		}

		reader.seek(Start(header.end_position.pos))?;

		Ok(ModelPrototype14 {
			visual_resource: header.visual_resource,
			misc_type: header.misc_type,
			animations,
			dyes,
		})
	}
}

impl DyePrototype14 {
	fn new(reader: &mut Cursor<Vec<u8>>, header: DyePrototypeHeader14) -> AkizukiResult<Self> {
		reader.seek(Start(header.relative_position.pos + header.tint_name_ids_offset))?;
		let tints = Vec::<StringId>::read_ne_args(
			reader,
			VecArgs {
				count: header.tint_count as usize,
				inner: <_>::default(),
			},
		)?;
		reader.seek(Start(header.relative_position.pos + header.tint_material_ids_offset))?;
		let materials = Vec::<ResourceId>::read_ne_args(
			reader,
			VecArgs {
				count: header.tint_count as usize,
				inner: <_>::default(),
			},
		)?;

		let mut map = HashMap::<StringId, ResourceId>::with_capacity(header.tint_count as usize);
		for i in 0..header.tint_count as usize {
			map.insert(tints[i], materials[i]);
		}

		Ok(DyePrototype14 {
			matter: header.matter_id,
			replaces: header.replaces_id,
			tints: map,
		})
	}
}
