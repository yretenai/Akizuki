// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use glam::{IVec3, Vec2, Vec3, Vec4};

use crate::space::vertex::*;

pub mod geometry;
pub mod vertex;

#[derive(Debug, Clone)]
pub enum IndexFormat {
	U16(Vec<u16>),
	U32(Vec<u32>),
}

#[derive(Debug, Clone)]
pub enum VertexFormat {
	XYZ(Vec<VertexXYZ>),
	XYZN(Vec<VertexXYZN>),
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
pub struct VertexStream {
	pub position: Vec<Vec3>,            // xyz
	pub normal: Option<Vec<Vec3>>,      // n
	pub texcoord: Option<Vec<Vec2>>,    // uv
	pub texcoord2: Option<Vec<Vec2>>,   // uv2
	pub bone_index: Option<Vec<IVec3>>, // iii
	pub bone_weight: Option<Vec<Vec3>>, // ww
	pub tangent: Option<Vec<Vec4>>,     // t
	pub binormal: Option<Vec<Vec3>>,    // b
	pub radius: Option<Vec<f32>>,       // r
	pub color: Option<Vec<Vec4>>,       // o?
	pub id: Option<Vec<u32>>,           // i?
}
