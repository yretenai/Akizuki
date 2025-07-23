// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use bytemuck::{Pod, Zeroable};
use glam::{IVec3, Vec3};
use half::f16;

use crate::space::vertex::vertex_helper::{unpack_bone_index, unpack_bone_weight, unpack_normal, unpack_uv};
use crate::space::vertex::{Vertex, VertexDecode, VertexStream};

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
pub struct VertexXYZNUVIIIWWR {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
	pub iii: [u8; 4],
	pub ww: [u8; 4],
	pub r: f32,
}

impl VertexDecode for Vec<VertexXYZNUVIIIWWR> {
	fn decode(&self) -> VertexStream {
		let mut stream: VertexStream = Default::default();

		stream.common.reserve(self.len());

		let mut bone_index: Vec<IVec3> = Vec::with_capacity(self.len());
		let mut bone_weight: Vec<Vec3> = Vec::with_capacity(self.len());
		let mut radius: Vec<f32> = Vec::with_capacity(self.len());

		for v in self.iter() {
			stream.common.push(Vertex {
				position: Vec3::from_array(v.xyz),
				normal: unpack_normal(v.n),
				texcoord: unpack_uv(v.uv),
			});

			bone_index.push(unpack_bone_index(v.iii));
			bone_weight.push(unpack_bone_weight(v.ww));
			radius.push(v.r);
		}

		stream.bone_index = Some(bone_index);
		stream.bone_weight = Some(bone_weight);
		stream.radius = Some(radius);

		stream
	}
}
