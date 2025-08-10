// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use glam::{IVec3, Vec2, Vec3};

use crate::space::VertexStream;
use crate::space::vertex::vertex_helper::{unpack_bone_index, unpack_bone_weight, unpack_normal, unpack_uv};
use crate::space::vertex::{VertexDecode, VertexXYZNUVIIIWW};

impl VertexDecode for Vec<VertexXYZNUVIIIWW> {
	fn decode(&self) -> VertexStream {
		let mut stream: VertexStream = Default::default();

		stream.position.reserve(self.len());
		let mut normal: Vec<Vec3> = Vec::with_capacity(self.len());
		let mut texcoord: Vec<Vec2> = Vec::with_capacity(self.len());
		let mut bone_index: Vec<IVec3> = Vec::with_capacity(self.len());
		let mut bone_weight: Vec<Vec3> = Vec::with_capacity(self.len());

		for v in self.iter() {
			stream.position.push(Vec3::from_array(v.xyz));
			normal.push(unpack_normal(v.n));
			texcoord.push(unpack_uv(v.uv));
			bone_index.push(unpack_bone_index(v.iii));
			bone_weight.push(unpack_bone_weight(v.ww));
		}

		stream.normal = Some(normal);
		stream.texcoord = Some(texcoord);
		stream.bone_index = Some(bone_index);
		stream.bone_weight = Some(bone_weight);

		stream
	}
}
