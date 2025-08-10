// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use glam::Vec3;

use crate::space::VertexStream;
use crate::space::vertex::{VertexDecode, VertexXYZ};

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
