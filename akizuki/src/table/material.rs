// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::error::{AkizukiError, AkizukiResult};
use crate::format::bigworld_data::BigWorldTableHeader;
use crate::identifiers::{ResourceId, StringId};
use crate::table::v14::material::MaterialPrototype14;
use crate::table::{BigWorldTableRecord, TableRecord};
use crate::{bigworld_table_check, bigworld_table_version};
use akizuki_macro::akizuki_id;
use std::collections::HashMap;

use glam::{Mat4, Vec2, Vec3, Vec4};
use std::io::Cursor;

#[derive(Debug)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
#[serde(tag = "version")]
pub enum MaterialPrototypeVersion {
	V14(MaterialPrototype14),
}

impl TableRecord for MaterialPrototypeVersion {
	fn new(reader: &mut Cursor<Vec<u8>>, header: &BigWorldTableHeader) -> AkizukiResult<Self> {
		if header.id != akizuki_id!("MaterialPrototype") {
			return Err(AkizukiError::UnsupportedTable(header.id));
		}

		bigworld_table_check!(MaterialPrototype14, MaterialPrototypeVersion::V14, reader, header);

		Err(AkizukiError::UnsupportedTableVersion(header.id, header.version))
	}

	fn is_supported(header: &BigWorldTableHeader) -> bool {
		bigworld_table_version!(MaterialPrototype14, header);
		false
	}
}

impl From<MaterialPrototypeVersion> for BigWorldTableRecord {
	fn from(value: MaterialPrototypeVersion) -> Self {
		BigWorldTableRecord::MaterialPrototype(value.into())
	}
}

impl MaterialPrototypeVersion {
	pub fn bools(&self) -> &HashMap<StringId, bool> {
		match self {
			MaterialPrototypeVersion::V14(v14) => &v14.bools,
		}
	}

	pub fn ints(&self) -> &HashMap<StringId, i32> {
		match self {
			MaterialPrototypeVersion::V14(v14) => &v14.ints,
		}
	}

	pub fn uints(&self) -> &HashMap<StringId, u32> {
		match self {
			MaterialPrototypeVersion::V14(v14) => &v14.uints,
		}
	}

	pub fn floats(&self) -> &HashMap<StringId, f32> {
		match self {
			MaterialPrototypeVersion::V14(v14) => &v14.floats,
		}
	}

	pub fn textures(&self) -> &HashMap<StringId, ResourceId> {
		match self {
			MaterialPrototypeVersion::V14(v14) => &v14.textures,
		}
	}

	pub fn vector2s(&self) -> &HashMap<StringId, Vec2> {
		match self {
			MaterialPrototypeVersion::V14(v14) => &v14.vector2s,
		}
	}

	pub fn vector3s(&self) -> &HashMap<StringId, Vec3> {
		match self {
			MaterialPrototypeVersion::V14(v14) => &v14.vector3s,
		}
	}

	pub fn vector4s(&self) -> &HashMap<StringId, Vec4> {
		match self {
			MaterialPrototypeVersion::V14(v14) => &v14.vector4s,
		}
	}

	pub fn matrices(&self) -> &HashMap<StringId, Mat4> {
		match self {
			MaterialPrototypeVersion::V14(v14) => &v14.matrices,
		}
	}

	pub fn fx_path(&self) -> ResourceId {
		match self {
			MaterialPrototypeVersion::V14(v14) => v14.fx_path,
		}
	}

	pub fn collision_flags(&self) -> u32 {
		match self {
			MaterialPrototypeVersion::V14(v14) => v14.collision_flags,
		}
	}

	pub fn sort_order(&self) -> i32 {
		match self {
			MaterialPrototypeVersion::V14(v14) => v14.sort_order,
		}
	}
}
