// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use glam::{Vec2, Vec3};

use crate::space::VertexStream;
use crate::space::vertex::vertex_helper::{unpack_normal, unpack_uv};
use crate::space::vertex::{VertexDecode, VertexXYZNUVR};

impl VertexDecode for Vec<VertexXYZNUVR> {
	fn decode(&self) -> VertexStream {
		let mut stream: VertexStream = Default::default();

		stream.position.reserve(self.len());
		let mut normal: Vec<Vec3> = Vec::with_capacity(self.len());
		let mut texcoord: Vec<Vec2> = Vec::with_capacity(self.len());
		let mut radius: Vec<f32> = Vec::with_capacity(self.len());

		for v in self.iter() {
			stream.position.push(Vec3::from_array(v.xyz));
			normal.push(unpack_normal(v.n));
			texcoord.push(unpack_uv(v.uv));
			radius.push(v.r);
		}

		stream.normal = Some(normal);
		stream.texcoord = Some(texcoord);
		stream.radius = Some(radius);

		stream
	}
}
