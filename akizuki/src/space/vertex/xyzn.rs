// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use glam::Vec3;

use crate::space::VertexStream;
use crate::space::vertex::vertex_helper::unpack_normal;
use crate::space::vertex::{VertexDecode, VertexXYZN};

impl VertexDecode for Vec<VertexXYZN> {
	fn decode(&self) -> VertexStream {
		let mut stream: VertexStream = Default::default();

		stream.position.reserve(self.len());
		let mut normal: Vec<Vec3> = Vec::with_capacity(self.len());

		for v in self.iter() {
			stream.position.push(Vec3::from_array(v.xyz));
			normal.push(unpack_normal(v.n));
		}

		stream.normal = Some(normal);

		stream
	}
}
