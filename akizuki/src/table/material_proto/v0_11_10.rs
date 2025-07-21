// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use std::collections::HashMap;
use std::io::SeekFrom::Start;
use std::io::{Cursor, Seek};

use akizuki_macro::BigWorldTable;
use binrw::{BinRead, BinReaderExt, PosValue, VecArgs};

use crate::bigworld_read_array;
use crate::bin_wrap::{FlagBool, Mat4, Vec2, Vec3, Vec4};
use crate::error::AkizukiResult;
use crate::identifiers::{ResourceId, StringId};

#[derive(BinRead, Debug)]
#[br()]
pub struct MaterialPrototypeHeader0_11_10 {
	pub relative_position: PosValue<()>,

	pub property_count: u16,
	pub bool_values_count: u8,
	pub int_values_count: u8,
	pub uint_values_count: u8,
	pub float_values_count: u8,
	pub texture_values_count: u8,
	pub vector2_values_count: u8,
	pub vector3_values_count: u8,
	pub vector4_values_count: u8,
	pub matrix_values_count: u8,
	#[br(pad_before = 5)]
	pub property_name_ids_offset: u64,
	pub property_ids_offset: u64,
	pub bool_values_offset: u64,
	pub int_values_offset: u64,
	pub uint_values_offset: u64,
	pub float_values_offset: u64,
	pub texture_values_offset: u64,
	pub vector2_values_offset: u64,
	pub vector3_values_offset: u64,
	pub vector4_values_offset: u64,
	pub matrix_values_offset: u64,
	pub fx_path_id: ResourceId,
	pub collision_flags: u32,
	pub sort_order: i32,

	pub end_position: PosValue<()>,
}

#[derive(BigWorldTable, Debug)]
#[table("MaterialPrototype", 0xa95414d5)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub struct MaterialPrototype0_11_10 {
	pub bools: HashMap<StringId, bool>,
	pub ints: HashMap<StringId, i32>,
	pub uints: HashMap<StringId, u32>,
	pub floats: HashMap<StringId, f32>,
	pub textures: HashMap<StringId, ResourceId>,
	pub vector2s: HashMap<StringId, glam::Vec2>,
	pub vector3s: HashMap<StringId, glam::Vec3>,
	pub vector4s: HashMap<StringId, glam::Vec4>,
	pub matrices: HashMap<StringId, glam::Mat4>,
	pub fx_path: ResourceId,
	pub collision_flags: u32,
	pub sort_order: i32,
}

enum MaterialPropertyType0_11_10 {
	Bool,
	Int,
	UInt,
	Float,
	Texture,
	Vector2,
	Vector3,
	Vector4,
	Matrix,
}

#[derive(BinRead, Debug)]
#[br(repr = u16)]
struct MaterialPropertyId0_11_10(u16);

impl MaterialPropertyId0_11_10 {
	pub fn property_type(&self) -> MaterialPropertyType0_11_10 {
		match self.0 & 0xf {
			0 => MaterialPropertyType0_11_10::Bool,
			1 => MaterialPropertyType0_11_10::Int,
			2 => MaterialPropertyType0_11_10::UInt,
			3 => MaterialPropertyType0_11_10::Float,
			4 => MaterialPropertyType0_11_10::Texture,
			5 => MaterialPropertyType0_11_10::Vector2,
			6 => MaterialPropertyType0_11_10::Vector3,
			7 => MaterialPropertyType0_11_10::Vector4,
			8 => MaterialPropertyType0_11_10::Matrix,
			_ => panic!("invalid type"),
		}
	}

	pub fn index(&self) -> usize {
		(self.0 >> 4) as usize
	}
}

impl From<u16> for MaterialPropertyId0_11_10 {
	fn from(value: u16) -> Self {
		Self(value)
	}
}

impl MaterialPrototype0_11_10 {
	pub fn new(reader: &mut Cursor<Vec<u8>>) -> AkizukiResult<Self> {
		let header = reader.read_ne::<MaterialPrototypeHeader0_11_10>()?;

		bigworld_read_array!(reader, header, property_names, property_count, property_name_ids_offset, StringId);
		bigworld_read_array!(reader, header, property_ids, property_count, property_ids_offset, MaterialPropertyId0_11_10);
		bigworld_read_array!(reader, header, bool_values, bool_values_count, bool_values_offset, FlagBool);
		bigworld_read_array!(reader, header, i32_values, int_values_count, int_values_offset, i32);
		bigworld_read_array!(reader, header, u32_values, uint_values_count, uint_values_offset, u32);
		bigworld_read_array!(reader, header, f32_values, float_values_count, float_values_offset, f32);
		bigworld_read_array!(reader, header, texture_values, texture_values_count, texture_values_offset, ResourceId);
		bigworld_read_array!(reader, header, vec2_values, vector2_values_count, int_values_offset, Vec2);
		bigworld_read_array!(reader, header, vec3_values, vector3_values_count, int_values_offset, Vec3);
		bigworld_read_array!(reader, header, vec4_values, vector4_values_count, int_values_offset, Vec4);
		bigworld_read_array!(reader, header, matrix_values, matrix_values_count, matrix_values_offset, Mat4);

		let mut bools = HashMap::<StringId, bool>::new();
		let mut ints = HashMap::<StringId, i32>::new();
		let mut uints = HashMap::<StringId, u32>::new();
		let mut floats = HashMap::<StringId, f32>::new();
		let mut textures = HashMap::<StringId, ResourceId>::new();
		let mut vector2s = HashMap::<StringId, glam::Vec2>::new();
		let mut vector3s = HashMap::<StringId, glam::Vec3>::new();
		let mut vector4s = HashMap::<StringId, glam::Vec4>::new();
		let mut matrices = HashMap::<StringId, glam::Mat4>::new();

		for (name, id) in property_names.iter().zip(property_ids) {
			match id.property_type() {
				MaterialPropertyType0_11_10::Bool => {
					bools.insert(*name, bool_values[id.index()].into());
				}
				MaterialPropertyType0_11_10::Int => {
					ints.insert(*name, i32_values[id.index()]);
				}
				MaterialPropertyType0_11_10::UInt => {
					uints.insert(*name, u32_values[id.index()]);
				}
				MaterialPropertyType0_11_10::Float => {
					floats.insert(*name, f32_values[id.index()]);
				}
				MaterialPropertyType0_11_10::Texture => {
					textures.insert(*name, texture_values[id.index()]);
				}
				MaterialPropertyType0_11_10::Vector2 => {
					vector2s.insert(*name, vec2_values[id.index()].into());
				}
				MaterialPropertyType0_11_10::Vector3 => {
					vector3s.insert(*name, vec3_values[id.index()].into());
				}
				MaterialPropertyType0_11_10::Vector4 => {
					vector4s.insert(*name, vec4_values[id.index()].into());
				}
				MaterialPropertyType0_11_10::Matrix => {
					matrices.insert(*name, matrix_values[id.index()].into());
				}
			}
		}

		reader.seek(Start(header.end_position.pos))?;

		Ok(MaterialPrototype0_11_10 {
			bools,
			ints,
			uints,
			floats,
			textures,
			vector2s,
			vector3s,
			vector4s,
			matrices,
			fx_path: header.fx_path_id,
			collision_flags: header.collision_flags,
			sort_order: header.sort_order,
		})
	}
}
