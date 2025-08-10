// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

pub mod vertex_helper;
pub mod xyz;
pub mod xyzn;
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

use binrw::BinRead;
use bytemuck::{Pod, Zeroable};
use half::f16;

use crate::space::{VertexFormat, VertexStream};

// todo: this can be easily macro'd
// bigworld_vertex!(XYZ, N, UV, UV2, TB, I) maybe?
#[derive(BinRead, Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
#[br()]
pub struct VertexXYZ {
	pub xyz: [f32; 3],
}

#[derive(BinRead, Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
#[br()]
pub struct VertexXYZN {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
}

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
pub struct VertexXYZNUV {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
}

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
pub struct VertexXYZNUV2IIIWWTB {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
	pub uv2: [f16; 2],
	pub iii: [u8; 4],
	pub ww: [u8; 4],
	pub t: [i8; 4],
	pub b: [i8; 4],
}

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
pub struct VertexXYZNUV2TB {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
	pub uv2: [f16; 2],
	pub t: [i8; 4],
	pub b: [i8; 4],
}

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
pub struct VertexXYZNUV2TBI {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
	pub uv2: [f16; 2],
	pub t: [i8; 4],
	pub b: [i8; 4],
	pub i: u32,
}

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
pub struct VertexXYZNUVIIIWW {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
	pub iii: [u8; 4],
	pub ww: [u8; 4],
}

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
pub struct VertexXYZNUVIIIWWR {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
	pub iii: [u8; 4],
	pub ww: [u8; 4],
	pub r: f32,
}

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
pub struct VertexXYZNUVIIIWWTB {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
	pub iii: [u8; 4],
	pub ww: [u8; 4],
	pub t: [i8; 4],
	pub b: [i8; 4],
}

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
pub struct VertexXYZNUVR {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
	pub r: f32,
}

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
pub struct VertexXYZNUVTB {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
	pub t: [i8; 4],
	pub b: [i8; 4],
}

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
pub struct VertexXYZNUVTBI {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
	pub t: [i8; 4],
	pub b: [i8; 4],
	pub i: u32,
}

#[derive(Debug, Copy, Clone, Pod, Zeroable)]
#[repr(C, packed(4))]
pub struct VertexXYZNUVTBOI {
	pub xyz: [f32; 3],
	pub n: [i8; 4],
	pub uv: [f16; 2],
	pub t: [i8; 4],
	pub b: [i8; 4],
	pub o: [u8; 4],
	pub i: u32,
}

pub trait VertexDecode {
	fn decode(&self) -> VertexStream;
}

impl VertexFormat {
	pub fn decode(&self) -> VertexStream {
		use VertexFormat::*;
		match self {
			XYZ(vert) => vert.decode(),
			XYZN(vert) => vert.decode(),
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
