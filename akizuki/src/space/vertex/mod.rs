// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

pub mod vertex_helper;
pub mod xyznuv;
pub mod xyznuv2iiiwwtb;
pub mod xyznuv2tb;
pub mod xyznuv2tbi;
pub mod xyznuviiiww;
pub mod xyznuviiiwwr;
pub mod xyznuviiiwwtb;
pub mod xyznuvr;
pub mod xyznuvtb;
pub mod xyznuvtbi;
pub mod xyznuvtboi;

use glam::{IVec3, Vec2, Vec3, Vec4};

use crate::space::vertex::xyznuv::VertexXYZNUV;
use crate::space::vertex::xyznuv2iiiwwtb::VertexXYZNUV2IIIWWTB;
use crate::space::vertex::xyznuv2tb::VertexXYZNUV2TB;
use crate::space::vertex::xyznuv2tbi::VertexXYZNUV2TBI;
use crate::space::vertex::xyznuviiiww::VertexXYZNUVIIIWW;
use crate::space::vertex::xyznuviiiwwr::VertexXYZNUVIIIWWR;
use crate::space::vertex::xyznuviiiwwtb::VertexXYZNUVIIIWWTB;
use crate::space::vertex::xyznuvr::VertexXYZNUVR;
use crate::space::vertex::xyznuvtb::VertexXYZNUVTB;
use crate::space::vertex::xyznuvtbi::VertexXYZNUVTBI;
use crate::space::vertex::xyznuvtboi::VertexXYZNUVTBOI;

pub trait VertexDecode {
	fn decode(&self) -> VertexStream;
}

#[derive(Debug)]
pub enum VertexFormat {
	XYZNUV(Vec<VertexXYZNUV>),
	XYZNUV2IIIWWTB(Vec<VertexXYZNUV2IIIWWTB>),
	XYZNUV2TB(Vec<VertexXYZNUV2TB>),
	XYZNUV2TBI(Vec<VertexXYZNUV2TBI>),
	XYZNUVIIIWW(Vec<VertexXYZNUVIIIWW>),
	XYZNUVIIIWWR(Vec<VertexXYZNUVIIIWWR>),
	XYZNUVIIIWWTB(Vec<VertexXYZNUVIIIWWTB>),
	XYZNUVR(Vec<VertexXYZNUVR>),
	XYZNUVTB(Vec<VertexXYZNUVTB>),
	XYZNUVTBI(Vec<VertexXYZNUVTBI>),
	XYZNUVTBOI(Vec<VertexXYZNUVTBOI>),
}

#[derive(Debug, Default, Clone)]
pub struct Vertex {
	pub position: Vec3, // xyz
	pub normal: Vec3,   // n
	pub texcoord: Vec2, // uv
}

#[derive(Debug, Default, Clone)]
pub struct VertexStream {
	pub common: Vec<Vertex>,            // xyznuv
	pub texcoord2: Option<Vec<Vec2>>,   // uv2
	pub bone_index: Option<Vec<IVec3>>, // iii
	pub bone_weight: Option<Vec<Vec3>>, // ww
	pub tangent: Option<Vec<Vec4>>,     // t
	pub binormal: Option<Vec<Vec3>>,    // b
	pub radius: Option<Vec<f32>>,       // r
	pub color: Option<Vec<Vec4>>,       // o?
	pub id: Option<Vec<u32>>,           // i?
}

impl VertexFormat {
	pub fn decode(&self) -> VertexStream {
		use VertexFormat::*;
		match self {
			XYZNUV(vert) => vert.decode(),
			XYZNUV2IIIWWTB(vert) => vert.decode(),
			XYZNUV2TB(vert) => vert.decode(),
			XYZNUV2TBI(vert) => vert.decode(),
			XYZNUVIIIWW(vert) => vert.decode(),
			XYZNUVIIIWWR(vert) => vert.decode(),
			XYZNUVIIIWWTB(vert) => vert.decode(),
			XYZNUVR(vert) => vert.decode(),
			XYZNUVTB(vert) => vert.decode(),
			XYZNUVTBI(vert) => vert.decode(),
			XYZNUVTBOI(vert) => vert.decode(),
		}
	}
}
