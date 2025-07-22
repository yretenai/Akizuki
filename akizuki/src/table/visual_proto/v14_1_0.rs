// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use std::collections::HashMap;
use std::io::SeekFrom::Start;
use std::io::{Cursor, Seek};

use akizuki_macro::BigWorldTable;
use binrw::{BinRead, BinReaderExt, PosValue, VecArgs};

use crate::bigworld_read_array;
use crate::bin_wrap::{BoundingBox, FlagBool};
use crate::error::AkizukiResult;
use crate::identifiers::{ResourceId, StringId};
use crate::table::skeleton::SkeletonPrototypeVersion;
use crate::table::skeleton_proto::v14_0_0::{SkeletonPrototype14_0_0, SkeletonPrototypeHeader14_0_0};
use crate::table::visual::*;

#[derive(BinRead, Debug)]
#[br()]
pub struct VisualPrototypeHeader14_1_0 {
	pub relative_position: PosValue<()>,

	pub skeleton_prototype: SkeletonPrototypeHeader14_0_0,
	pub merged_geometry_path: ResourceId,
	pub is_underwater_model: FlagBool,
	pub is_abovewater_model: FlagBool,
	pub sets_count: u16,
	pub lod_count: u8,
	#[br(pad_before = 3)]
	pub bounding_box: BoundingBox,
	pub sets_offset: u64,
	pub lods_offset: u64,

	pub end_position: PosValue<()>,
}

#[derive(BinRead, Debug)]
#[br()]
pub struct RenderSetPrototypeHeader14_1_0 {
	pub relative_position: PosValue<()>,

	pub name: StringId,
	pub material_name: StringId,
	pub vertices_name: StringId,
	pub indices_name: StringId,
	pub material_resource: ResourceId,
	pub is_skinned: FlagBool,
	pub node_count: u8,
	#[br(pad_before = 6)]
	pub node_offset: u64,
}

#[derive(BinRead, Debug)]
#[br()]
pub struct LODPrototypeHeader14_1_0 {
	pub relative_position: PosValue<()>,

	pub extent: f32,
	pub cast_shadows: FlagBool,
	#[br(pad_before = 1)]
	pub render_set_count: u16,
	pub render_set_offset: u64,
}

#[derive(BigWorldTable, Debug)]
#[table("VisualPrototype", 0x3167064b)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize), serde_with::serde_as)]
pub struct VisualPrototype14_1_0 {
	pub skeleton_prototype: SkeletonPrototypeVersion,
	pub merged_geometry_path: ResourceId,
	pub is_underwater_model: bool,
	pub is_abovewater_model: bool,
	pub bounding_box: BoundingBox,
	#[serde_as(as = "HashMap<u32, _>")]
	pub render_sets: HashMap<StringId, RenderSetPrototypeVersion>,
	pub lods: Vec<LODPrototypeVersion>,
}

#[derive(Debug)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub struct RenderSetPrototype14_1_0 {
	pub name: StringId,
	pub material_name: StringId,
	pub vertices_name: StringId,
	pub indices_name: StringId,
	pub material_resource: ResourceId,
	pub is_skinned: bool,
	pub nodes: Vec<StringId>,
}

#[derive(Debug)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub struct LODPrototype14_1_0 {
	pub extent: f32,
	pub cast_shadows: bool,
	pub render_sets: Vec<StringId>,
}

impl VisualPrototype14_1_0 {
	pub fn new(reader: &mut Cursor<Vec<u8>>) -> AkizukiResult<Self> {
		let header = reader.read_ne::<VisualPrototypeHeader14_1_0>()?;
		reader.set_position(header.relative_position.pos);
		let skeleton_prototype = SkeletonPrototype14_0_0::new(reader)?;

		bigworld_read_array!(reader, header, lod_headers, lod_count, lods_offset, LODPrototypeHeader14_1_0);
		bigworld_read_array!(reader, header, render_set_headers, sets_count, sets_offset, RenderSetPrototypeHeader14_1_0);

		let mut lods = Vec::<LODPrototypeVersion>::with_capacity(header.lod_count as usize);
		for lod_header in lod_headers {
			lods.push(LODPrototypeVersion::V14_1_0(LODPrototype14_1_0::new(reader, lod_header)?));
		}

		let mut render_sets = Vec::<RenderSetPrototypeVersion>::with_capacity(header.sets_count as usize);
		for render_set_header in render_set_headers {
			render_sets.push(RenderSetPrototypeVersion::V14_1_0(RenderSetPrototype14_1_0::new(reader, render_set_header)?));
		}

		reader.seek(Start(header.end_position.pos))?;

		Ok(VisualPrototype14_1_0 {
			skeleton_prototype: SkeletonPrototypeVersion::V14_0_0(skeleton_prototype),
			merged_geometry_path: header.merged_geometry_path,
			is_underwater_model: header.is_underwater_model.into(),
			is_abovewater_model: header.is_abovewater_model.into(),
			bounding_box: header.bounding_box,
			render_sets: render_sets.into_iter().map(|render_set| (render_set.name(), render_set)).collect(),
			lods,
		})
	}
}

impl LODPrototype14_1_0 {
	fn new(reader: &mut Cursor<Vec<u8>>, header: LODPrototypeHeader14_1_0) -> AkizukiResult<Self> {
		bigworld_read_array!(reader, header, render_sets, render_set_count, render_set_offset, StringId);

		Ok(LODPrototype14_1_0 {
			extent: header.extent,
			cast_shadows: header.cast_shadows.into(),
			render_sets,
		})
	}
}

impl RenderSetPrototype14_1_0 {
	fn new(reader: &mut Cursor<Vec<u8>>, header: RenderSetPrototypeHeader14_1_0) -> AkizukiResult<Self> {
		bigworld_read_array!(reader, header, nodes, node_count, node_offset, StringId);

		Ok(RenderSetPrototype14_1_0 {
			name: header.name,
			material_name: header.material_name,
			vertices_name: header.vertices_name,
			indices_name: header.indices_name,
			material_resource: header.material_resource,
			is_skinned: header.is_skinned.into(),
			nodes,
		})
	}
}
