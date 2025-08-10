// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use binrw::{BinRead, PosValue};

use crate::bin_wrap::{BoundingBox, FlagBool};
use crate::identifiers::StringId;
use crate::space::vertex::{VertexXYZ, VertexXYZN};

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryFileHeader {
	pub vertex_buffer_count: u32,
	pub index_buffer_count: u32,
	pub vertex_name_count: u32,
	pub index_name_count: u32,
	pub collision_buffer_count: u32,
	pub armor_buffer_count: u32,
	pub vertex_name_offset: u64,
	pub index_name_offset: u64,
	pub vertex_buffer_offset: u64,
	pub index_buffer_offset: u64,
	pub collision_buffer_offset: u64,
	pub armor_buffer_offset: u64,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryNameRaw {
	pub name: StringId,
	pub buffer_id: u16,
	pub mesh_id: u16,
	pub buffer_offset: u32,
	pub buffer_length: u32,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryVertexBufferHeader {
	pub relative_position: PosValue<()>,

	pub buffer_offset: u64,
	pub vertex_format_length: u64,
	pub vertex_format_offset: u64,
	pub buffer_length: u32,
	pub vertex_stride: u16,
	pub is_skinned: FlagBool,
	pub has_tangent: FlagBool,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryIndexBufferHeader {
	pub relative_position: PosValue<()>,

	pub buffer_offset: u64,
	pub buffer_length: u32,
	pub unknown: u16,
	pub index_stride: u16,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryArmorPlateHeader {
	pub thickness: u16,
	pub type_id: u16,
	pub bounding_box: BoundingBox,
	pub vertex_count: u32,
	#[br(args { count: vertex_count as usize, })]
	pub vertices: Vec<VertexXYZN>,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryArmorSurfaceHeader {
	pub id: u32,
	pub bounding_box: BoundingBox,
	pub count: u32,
	#[br(args { count: count as usize, })]
	pub plates: Vec<GeometryArmorPlateHeader>,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryMiscBufferHeader {
	pub relative_position: PosValue<()>,

	// armor and collision use this.
	pub buffer_length: u64,
	pub name_length: u64,
	pub name_offset: u64,
	pub buffer_offset: u64,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryCollisionFaceHeader {
	pub count: u32,
	#[br(args { count: count as usize, })]
	pub inside: Vec<u32>,
	#[br(args { count: count as usize, })]
	pub outside: Vec<u32>,
	// added in 24/02/2012, only other valid version is 26/09/2011
	pub unknown: u32,
	// using MMDDYYYY as a version number is diabolical, YYYYMMDD can be compared
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryCollisionBufferHeader {
	pub vertex_count: u32,
	#[br(args { count: vertex_count as usize, })]
	pub vertices: Vec<VertexXYZ>,

	pub edge_count: u32,
	#[br(args { count: edge_count as usize, })]
	pub edges: Vec<crate::bin_wrap::IVec2>,

	pub face_count: u32,
	#[br(args { count: edge_count as usize, })]
	pub faces: Vec<GeometryCollisionFaceHeader>,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryCollisionConvexHullHeader {
	pub version: u32,

	pub mesh_count: u32,
	#[br(args { count: mesh_count as usize, })]
	pub meshes: Vec<GeometryCollisionBufferHeader>,
}
