// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::bin_wrap::{BoundingBox, FlagBool, Mat4};
use crate::error::AkizukiResult;
use crate::identifiers::{ResourceId, StringId};
use crate::table::visual::*;
use akizuki_macro::BigWorldTable;

use binrw::{BinRead, BinReaderExt, PosValue, VecArgs};

use std::collections::HashMap;
use std::io::SeekFrom::Start;
use std::io::{Cursor, Seek};

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct VisualPrototypeHeader14 {
	pub relative_position: PosValue<()>,

	pub skeleton_prototype: SkeletonPrototypeHeader14,
	pub merged_geometry_path: ResourceId,
	pub is_underwater_model: FlagBool,
	pub is_abovewater_model: FlagBool,
	pub render_sets_count: u16,
	pub lod_count: u8,
	#[br(pad_before = 3)]
	pub bounding_box: BoundingBox,
	pub render_sets_offset: u64,
	pub lods_offset: u64,

	pub end_position: PosValue<()>,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct SkeletonPrototypeHeader14 {
	pub relative_position: PosValue<()>,

	pub node_count: u32,
	#[br(pad_before = 4)]
	pub name_map_id_offset: u64,
	pub name_map_node_offset: u64,
	pub name_ids_offset: u64,
	pub matrices_offset: u64,
	pub parent_ids_offset: u64,

	pub end_position: PosValue<()>,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct RenderSetPrototypeHeader14 {
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

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct LODPrototypeHeader14 {
	pub relative_position: PosValue<()>,

	pub extent: f32,
	pub cast_shadows: FlagBool,
	#[br(pad_before = 1)]
	pub render_set_count: u16,
	pub render_set_offset: u64,
}

#[derive(Debug, BigWorldTable)]
#[table(VisualPrototype, 0x3167064b)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub struct VisualPrototype14 {
	pub skeleton_prototype: SkeletonPrototype14,
	pub merged_geometry_path: ResourceId,
	pub is_underwater_model: bool,
	pub is_abovewater_model: bool,
	pub bounding_box: BoundingBox,
	pub render_sets: HashMap<StringId, RenderSetPrototype14>,
	pub lods: Vec<LODPrototype14>,
}

#[derive(Debug)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub struct SkeletonPrototype14 {
	pub names: Vec<StringId>,
	pub matrices: Vec<Mat4>,
	pub parent_ids: Vec<u16>,
}

#[derive(Debug)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub struct RenderSetPrototype14 {
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
pub struct LODPrototype14 {
	pub extent: f32,
	pub cast_shadows: bool,
	pub render_sets: Vec<StringId>,
}

impl VisualPrototype14 {
	pub fn new(reader: &mut Cursor<Vec<u8>>) -> AkizukiResult<Self> {
		let header = reader.read_ne::<VisualPrototypeHeader14>()?;

		let skeleton_prototype = SkeletonPrototype14::new(reader, header.skeleton_prototype)?;

		reader.seek(Start(header.relative_position.pos + header.lods_offset))?;
		let lod_headers = Vec::<LODPrototypeHeader14>::read_ne_args(
			reader,
			VecArgs {
				count: header.lod_count as usize,
				inner: <_>::default(),
			},
		)?;

		let mut lods = Vec::<LODPrototype14>::with_capacity(header.lod_count as usize);
		for lod_header in lod_headers {
			lods.push(LODPrototype14::new(reader, lod_header)?);
		}

		reader.seek(Start(header.relative_position.pos + header.render_sets_offset))?;
		let render_set_headers = Vec::<RenderSetPrototypeHeader14>::read_ne_args(
			reader,
			VecArgs {
				count: header.render_sets_count as usize,
				inner: <_>::default(),
			},
		)?;

		let mut render_sets = Vec::<RenderSetPrototype14>::with_capacity(header.render_sets_count as usize);
		for render_set_header in render_set_headers {
			render_sets.push(RenderSetPrototype14::new(reader, render_set_header)?);
		}

		reader.seek(Start(header.end_position.pos))?;

		Ok(VisualPrototype14 {
			skeleton_prototype,
			merged_geometry_path: header.merged_geometry_path,
			is_underwater_model: header.is_underwater_model.into(),
			is_abovewater_model: header.is_abovewater_model.into(),
			bounding_box: header.bounding_box,
			render_sets: render_sets
				.into_iter()
				.map(|render_set| (render_set.name, render_set))
				.collect(),
			lods,
		})
	}
}

impl SkeletonPrototype14 {
	fn new(reader: &mut Cursor<Vec<u8>>, header: SkeletonPrototypeHeader14) -> AkizukiResult<Self> {
		reader.seek(Start(header.relative_position.pos + header.name_ids_offset))?;

		let names = Vec::<StringId>::read_ne_args(
			reader,
			VecArgs {
				count: header.node_count as usize,
				inner: <_>::default(),
			},
		)?;

		let matrices = Vec::<Mat4>::read_ne_args(
			reader,
			VecArgs {
				count: header.node_count as usize,
				inner: <_>::default(),
			},
		)?;

		let parent_ids = Vec::<u16>::read_ne_args(
			reader,
			VecArgs {
				count: header.node_count as usize,
				inner: <_>::default(),
			},
		)?;

		Ok(SkeletonPrototype14 {
			names,
			matrices,
			parent_ids,
		})
	}
}

impl LODPrototype14 {
	fn new(reader: &mut Cursor<Vec<u8>>, header: LODPrototypeHeader14) -> AkizukiResult<Self> {
		reader.seek(Start(header.relative_position.pos + header.render_set_offset))?;

		let render_sets = Vec::<StringId>::read_ne_args(
			reader,
			VecArgs {
				count: header.render_set_count as usize,
				inner: <_>::default(),
			},
		)?;

		Ok(LODPrototype14 {
			extent: header.extent,
			cast_shadows: header.cast_shadows.into(),
			render_sets,
		})
	}
}

impl RenderSetPrototype14 {
	fn new(reader: &mut Cursor<Vec<u8>>, header: RenderSetPrototypeHeader14) -> AkizukiResult<Self> {
		reader.seek(Start(header.relative_position.pos + header.node_offset))?;

		let nodes = Vec::<StringId>::read_ne_args(
			reader,
			VecArgs {
				count: header.node_count as usize,
				inner: <_>::default(),
			},
		)?;

		Ok(RenderSetPrototype14 {
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

impl From<VisualPrototype14> for VisualPrototype {
	fn from(val: VisualPrototype14) -> VisualPrototype {
		VisualPrototype {
			skeleton_prototype: Some(val.skeleton_prototype.into()),
			merged_geometry_path: Some(val.merged_geometry_path),
			is_underwater_model: Some(val.is_underwater_model),
			is_abovewater_model: Some(val.is_abovewater_model),
			bounding_box: Some(val.bounding_box),
			render_sets: Some(val.render_sets.into_iter().map(|kv| (kv.0, kv.1.into())).collect()),
			lods: Some(val.lods.into_iter().map(Into::into).collect()),
		}
	}
}

impl From<SkeletonPrototype14> for SkeletonPrototype {
	fn from(val: SkeletonPrototype14) -> SkeletonPrototype {
		SkeletonPrototype {
			names: Some(val.names),
			matices: Some(val.matrices),
			parent_ids: Some(val.parent_ids),
		}
	}
}

impl From<RenderSetPrototype14> for RenderSetPrototype {
	fn from(val: RenderSetPrototype14) -> RenderSetPrototype {
		RenderSetPrototype {
			name: Some(val.name),
			material_name: Some(val.material_name),
			vertices_name: Some(val.vertices_name),
			indices_name: Some(val.indices_name),
			material_resource: Some(val.material_resource),
			is_skinned: Some(val.is_skinned),
			nodes: Some(val.nodes),
		}
	}
}

impl From<LODPrototype14> for LODPrototype {
	fn from(val: LODPrototype14) -> LODPrototype {
		LODPrototype {
			extent: Some(val.extent),
			cast_shadows: Some(val.cast_shadows),
			render_sets: Some(val.render_sets),
		}
	}
}
