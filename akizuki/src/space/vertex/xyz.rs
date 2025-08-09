// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use bytemuck::{Pod, Zeroable};
use glam::Vec3;

use crate::space::vertex::vertex_helper::unpack_normal;
use crate::space::vertex::{VertexDecode, VertexStream};

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
pub struct VertexXYZ {
	pub xyz: [f32; 3],
}

impl VertexDecode for Vec<VertexXYZ> {
	fn decode(&self) -> VertexStream {
		let mut stream: VertexStream = Default::default();

		stream.position.reserve(self.len());

		for v in self.iter() {
			stream.position.push(Vec3::from_array(v.xyz));
		}

		stream
	}
}
