// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use binrw::{BinRead, PosValue};
use glam::Vec3;

use crate::bin_wrap::{BoundingBox, FlagBool};
use crate::identifiers::StringId;
use crate::space::vertex::{VertexDecode, VertexStream};

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryFileHeader {
	pub relative_position: PosValue<()>,

	pub buffer_size: u64,
	pub name_size: u64,
	pub name_offset: u64,
	pub buffer_offset: u64,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryHeader {
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
pub struct GeometryName {
	pub name: StringId,
	pub buffer_id: u16,
	pub mesh_id: u16,
	pub buffer_offset: u32,
	pub buffer_length: u32,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryVertexBuffer {
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
pub struct GeometryIndexBuffer {
	pub relative_position: PosValue<()>,

	pub buffer_offset: u64,
	pub buffer_length: u32,
	pub unknown: u16,
	pub vertex_stride: u16,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryArmorSurface {
	pub thickness: u16,
	pub id: u16,
	pub bounding_box: BoundingBox,
	pub vertex_count: u32,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryMiscBuffer {
	// armor and collision use this.
	pub buffer_length: u64,
	pub name_length: u64,
	pub name_offset: u64,
	pub buffer_offset: u64,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryCollisionFace {
	pub count: u32,
	#[br(args { count: count as usize, })]
	pub inside: Vec<u32>,
	#[br(args { count: count as usize, })]
	pub outside: Vec<u32>,
	// added in 24/02/2012, only other valid version is 26/09/2011
	pub unknown_flag: u32,
	// using MMDDYYYY as a version number is diabolical, YYYYMMDD can be compared
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryCollisionBuffer {
	pub vertex_count: u32,
	#[br(args { count: vertex_count as usize, })]
	pub vertices: Vec<crate::bin_wrap::Vec3>,

	pub edge_count: u32,
	#[br(args { count: edge_count as usize, })]
	pub edges: Vec<crate::bin_wrap::IVec2>,

	pub face_count: u32,
	#[br(args { count: edge_count as usize, })]
	pub faces: Vec<GeometryCollisionFace>,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct GeometryCollisionConvexHull {
	pub version: u32,

	pub mesh_count: u32,
	#[br(args { count: mesh_count as usize, })]
	pub meshes: Vec<GeometryCollisionBuffer>,
}

impl VertexDecode for GeometryCollisionBuffer {
	fn decode(&self) -> VertexStream {
		let mut stream: VertexStream = Default::default();

		let vec: Vec<Vec3> = self.vertices.iter().map(|x| x.clone().into()).collect();
		stream.position.extend(vec);

		stream
	}
}
