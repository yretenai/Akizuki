// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use bytemuck::{Pod, Zeroable};
use glam::{IVec3, Vec2, Vec3, Vec4};
use half::f16;

use crate::space::vertex::vertex_helper::{unpack_bone_index, unpack_bone_weight, unpack_normal, unpack_tangent, unpack_uv};
use crate::space::vertex::{Vertex, VertexDecode, VertexStream};

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed)]
pub struct VertexXYZNUV2IIIWWTB {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
	pub uv2: [f16; 2],
	pub iii: [u8; 4],
	pub ww: [u8; 4],
	pub t: [i8; 4],
	pub b: [i8; 4],
}

impl VertexDecode for Vec<VertexXYZNUV2IIIWWTB> {
	fn decode(&self) -> VertexStream {
		let mut stream: VertexStream = Default::default();

		stream.common.reserve(self.len());

		let mut texcoord: Vec<Vec2> = Vec::with_capacity(self.len());
		let mut bone_index: Vec<IVec3> = Vec::with_capacity(self.len());
		let mut bone_weight: Vec<Vec3> = Vec::with_capacity(self.len());
		let mut tangent: Vec<Vec4> = Vec::with_capacity(self.len());
		let mut binormal: Vec<Vec3> = Vec::with_capacity(self.len());

		for v in self.iter() {
			stream.common.push(Vertex {
				position: Vec3::from_array(v.xyz),
				normal: unpack_normal(v.n),
				texcoord: unpack_uv(v.uv),
			});

			texcoord.push(unpack_uv(v.uv2));
			bone_index.push(unpack_bone_index(v.iii));
			bone_weight.push(unpack_bone_weight(v.ww));
			tangent.push(unpack_tangent(v.t));
			binormal.push(unpack_normal(v.b));
		}

		stream.texcoord2 = Some(texcoord);
		stream.bone_index = Some(bone_index);
		stream.bone_weight = Some(bone_weight);
		stream.tangent = Some(tangent);
		stream.binormal = Some(binormal);

		stream
	}
}
