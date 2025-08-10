// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use glam::{Vec2, Vec3, Vec4};

use crate::space::VertexStream;
use crate::space::vertex::vertex_helper::{unpack_normal, unpack_tangent, unpack_uv};
use crate::space::vertex::{VertexDecode, VertexXYZNUV2TB};

impl VertexDecode for Vec<VertexXYZNUV2TB> {
	fn decode(&self) -> VertexStream {
		let mut stream: VertexStream = Default::default();

		let mut normal: Vec<Vec3> = Vec::with_capacity(self.len());
		let mut texcoord: Vec<Vec2> = Vec::with_capacity(self.len());
		let mut texcoord2: Vec<Vec2> = Vec::with_capacity(self.len());
		let mut tangent: Vec<Vec4> = Vec::with_capacity(self.len());
		let mut binormal: Vec<Vec3> = Vec::with_capacity(self.len());

		for v in self.iter() {
			stream.position.push(Vec3::from_array(v.xyz));
			normal.push(unpack_normal(v.n));
			texcoord.push(unpack_uv(v.uv));
			texcoord2.push(unpack_uv(v.uv2));
			tangent.push(unpack_tangent(v.t));
			binormal.push(unpack_normal(v.b));
		}

		stream.normal = Some(normal);
		stream.texcoord = Some(texcoord);
		stream.texcoord2 = Some(texcoord2);
		stream.tangent = Some(tangent);
		stream.binormal = Some(binormal);

		stream
	}
}
