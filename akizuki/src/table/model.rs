// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::format::bigworld_table::{DyePrototypeHeader, ModelMiscType, ModelPrototypeHeader};
use crate::identifiers::{ResourceId, StringId};
use binrw::BinRead;
use binrw::{BinReaderExt, BinResult, VecArgs};
use std::io::SeekFrom::Start;
use std::io::{Cursor, Seek};

#[derive(Debug)]
pub struct ModelPrototype {
	pub visual_prototype: ResourceId,
	pub misc_type: ModelMiscType,
	pub animations: Vec<ResourceId>,
	pub dyes: Vec<DyePrototype>,
}

#[derive(Debug)]
pub struct DyePrototype {
	pub matter: StringId,
	pub replaces: StringId,
	pub tints: Vec<StringId>,
	pub materials: Vec<ResourceId>,
}

impl ModelPrototype {
	pub fn new(mut reader: &mut Cursor<&[u8]>) -> BinResult<Self> {
		let header = reader.read_ne::<ModelPrototypeHeader>()?;

		reader.seek(Start(header.relative_position.pos + header.animation_offset))?;
		let animations = Vec::<ResourceId>::read_ne_args(&mut reader, VecArgs { count: header.animation_count as usize, inner: <_>::default() })?;

		reader.seek(Start(header.relative_position.pos + header.dye_offset))?;
		let dye_headers = Vec::<DyePrototypeHeader>::read_ne_args(&mut reader, VecArgs { count: header.dye_count as usize, inner: <_>::default() })?;
		let mut dyes = Vec::<DyePrototype>::with_capacity(header.dye_count as usize);

		for dye_header in dye_headers {
			dyes.push(DyePrototype::new(reader, dye_header)?);
		}

		Ok(ModelPrototype { visual_prototype: header.visual_resource_id, misc_type: header.misc_type, animations, dyes })
	}
}

impl DyePrototype {
	fn new(mut reader: &mut Cursor<&[u8]>, header: DyePrototypeHeader) -> BinResult<Self> {
		reader.seek(Start(header.relative_position.pos + header.tint_name_ids_offset))?;
		let tints = Vec::<StringId>::read_ne_args(&mut reader, VecArgs { count: header.tint_count as usize, inner: <_>::default() })?;
		reader.seek(Start(header.relative_position.pos + header.tint_material_ids_offset))?;
		let materials = Vec::<ResourceId>::read_ne_args(&mut reader, VecArgs { count: header.tint_count as usize, inner: <_>::default() })?;

		Ok(DyePrototype { matter: header.matter_id, replaces: header.replaces_id, tints, materials })
	}
}
