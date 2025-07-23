// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use bytemuck::{Pod, Zeroable};
use glam::Vec3;
use half::f16;

use crate::space::vertex::vertex_helper::{unpack_normal, unpack_uv};
use crate::space::vertex::{Vertex, VertexDecode, VertexStream};

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed)]
pub struct VertexXYZNUV {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
}

impl VertexDecode for Vec<VertexXYZNUV> {
	fn decode(&self) -> VertexStream {
		let mut stream: VertexStream = Default::default();

		stream.common.reserve(self.len());

		for v in self.iter() {
			stream.common.push(Vertex {
				position: Vec3::from_array(v.xyz),
				normal: unpack_normal(v.n),
				texcoord: unpack_uv(v.uv),
			});
		}

		stream
	}
}
