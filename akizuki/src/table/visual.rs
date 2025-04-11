// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::error::{AkizukiError, AkizukiResult};
use crate::format::bigworld_data::BigWorldTableHeader;
use crate::identifiers::{ResourceId, StringId};
use crate::table::{BigWorldTableRecord, TableRecord, v14};
use akizuki_macro::{akizuki_id, bigworld_table_check, bigworld_table_version};

use crate::bin_wrap::{BoundingBox, Mat4};
use std::collections::HashMap;
use std::io::Cursor;

// everything is an option because these are the sum of all versions.

#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub struct VisualPrototype {
	pub skeleton_prototype: Option<SkeletonPrototype>,
	pub merged_geometry_path: Option<ResourceId>,
	pub is_underwater_model: Option<bool>,
	pub is_abovewater_model: Option<bool>,
	pub bounding_box: Option<BoundingBox>,
	pub render_sets: Option<HashMap<StringId, RenderSetPrototype>>,
	pub lods: Option<Vec<LODPrototype>>,
}

#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub struct SkeletonPrototype {
	pub names: Option<Vec<StringId>>,
	pub matices: Option<Vec<Mat4>>,
	pub parent_ids: Option<Vec<u16>>,
}

#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub struct RenderSetPrototype {
	pub name: Option<StringId>,
	pub material_name: Option<StringId>,
	pub vertices_name: Option<StringId>,
	pub indices_name: Option<StringId>,
	pub material_resource: Option<ResourceId>,
	pub is_skinned: Option<bool>,
	pub nodes: Option<Vec<StringId>>,
}

#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub struct LODPrototype {
	pub extent: Option<f32>,
	pub cast_shadows: Option<bool>,
	pub render_sets: Option<Vec<StringId>>,
}

impl TableRecord for VisualPrototype {
	fn new(reader: &mut Cursor<Vec<u8>>, header: &BigWorldTableHeader) -> AkizukiResult<Self> {
		if header.id != akizuki_id!("VisualPrototype") {
			return Err(AkizukiError::UnsupportedTable(header.id));
		}

		bigworld_table_version!(v14::visual::VisualPrototype14, reader, header);

		Err(AkizukiError::UnsupportedTableVersion(header.id, header.version))
	}

	fn is_supported(header: &BigWorldTableHeader) -> bool {
		bigworld_table_check!(v14::visual::VisualPrototype14, header);
		false
	}
}

impl From<VisualPrototype> for BigWorldTableRecord {
	fn from(value: VisualPrototype) -> Self {
		BigWorldTableRecord::VisualPrototype(value)
	}
}

impl From<SkeletonPrototype> for BigWorldTableRecord {
	fn from(value: SkeletonPrototype) -> Self {
		BigWorldTableRecord::SkeletonPrototype(value)
	}
}
