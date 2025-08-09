// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use bytemuck::{Pod, Zeroable};
use glam::{Vec2, Vec3, Vec4};
use half::f16;

use crate::space::vertex::vertex_helper::{unpack_normal, unpack_tangent, unpack_uv};
use crate::space::vertex::{VertexDecode, VertexStream};

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
pub struct VertexXYZNUVTB {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
	pub t: [i8; 4],
	pub b: [i8; 4],
}

impl VertexDecode for Vec<VertexXYZNUVTB> {
	fn decode(&self) -> VertexStream {
		let mut stream: VertexStream = Default::default();

		stream.position.reserve(self.len());
		let mut normal: Vec<Vec3> = Vec::with_capacity(self.len());
		let mut texcoord: Vec<Vec2> = Vec::with_capacity(self.len());
		let mut tangent: Vec<Vec4> = Vec::with_capacity(self.len());
		let mut binormal: Vec<Vec3> = Vec::with_capacity(self.len());

		for v in self.iter() {
			stream.position.push(Vec3::from_array(v.xyz));
			normal.push(unpack_normal(v.n));
			texcoord.push(unpack_uv(v.uv));
			tangent.push(unpack_tangent(v.t));
			binormal.push(unpack_normal(v.b));
		}

		stream.normal = Some(normal);
		stream.texcoord = Some(texcoord);
		stream.tangent = Some(tangent);
		stream.binormal = Some(binormal);

		stream
	}
}
