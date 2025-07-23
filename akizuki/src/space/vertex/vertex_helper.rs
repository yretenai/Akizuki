// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use glam::{IVec3, Vec2, Vec3, Vec4};
use half::f16;

pub fn i8norm(value: i8) -> f32 {
	let unsigned = (value as u8) ^ 0xff;
	if unsigned > 0x7f { -((unsigned & 0x7f) as f32 / 127.0) } else { (unsigned ^ 0x7f) as f32 / 127.0 }
}

pub fn u8norm(value: u8) -> f32 {
	(value as f32) / 255.0
}

pub fn unpack_uv(uv: [f16; 2]) -> Vec2 {
	let u: f32 = uv[0].into();
	let v: f32 = uv[1].into();
	Vec2::new(u + 0.5, v + 0.5)
}

pub fn unpack_normal(packed: [i8; 4]) -> Vec3 {
	Vec3::new(i8norm(packed[0]), i8norm(packed[1]), i8norm(packed[2]))
}

pub fn unpack_tangent(packed: [i8; 4]) -> Vec4 {
	Vec4::new(i8norm(packed[0]), i8norm(packed[1]), i8norm(packed[2]), i8norm(packed[3]))
}

pub fn unpack_color(packed: [u8; 4]) -> Vec4 {
	Vec4::new(u8norm(packed[0]), u8norm(packed[1]), u8norm(packed[2]), u8norm(packed[3]))
}

pub fn unpack_bone_weight(packed: [u8; 4]) -> Vec3 {
	let x = u8norm(packed[0]);
	let y = u8norm(if packed[1] >= packed[0] { packed[0] - packed[1] } else { packed[1] });
	let z = 1.0 - x - y;
	Vec3::new(x, y, if z > 0.0 { z } else { 0.0 })
}

pub fn unpack_bone_index(packed: [u8; 4]) -> IVec3 {
	IVec3::new((packed[0] / 3) as i32, (packed[1] / 3) as i32, (packed[2] / 3) as i32)
}
