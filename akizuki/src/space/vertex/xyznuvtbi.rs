// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use bytemuck::{Pod, Zeroable};
use glam::{Vec3, Vec4};
use half::f16;

use crate::space::vertex::vertex_helper::{unpack_normal, unpack_tangent, unpack_uv};
use crate::space::vertex::{Vertex, VertexDecode, VertexStream};

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed)]
pub struct VertexXYZNUVTBI {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
	pub t: [i8; 4],
	pub b: [i8; 4],
	pub i: u32,
}

impl VertexDecode for Vec<VertexXYZNUVTBI> {
	fn decode(&self) -> VertexStream {
		let mut stream: VertexStream = Default::default();

		stream.common.reserve(self.len());

		let mut tangent: Vec<Vec4> = Vec::with_capacity(self.len());
		let mut binormal: Vec<Vec3> = Vec::with_capacity(self.len());
		let mut id: Vec<u32> = Vec::with_capacity(self.len());

		for v in self.iter() {
			stream.common.push(Vertex {
				position: Vec3::from_array(v.xyz),
				normal: unpack_normal(v.n),
				texcoord: unpack_uv(v.uv),
			});

			tangent.push(unpack_tangent(v.t));
			binormal.push(unpack_normal(v.b));
			id.push(v.i);
		}

		stream.tangent = Some(tangent);
		stream.binormal = Some(binormal);
		stream.id = Some(id);

		stream
	}
}
