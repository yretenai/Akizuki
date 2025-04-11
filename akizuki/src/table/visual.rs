// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::bin_wrap::{BoundingBox, Mat4};
use crate::error::{AkizukiError, AkizukiResult};
use crate::format::bigworld_data::BigWorldTableHeader;
use crate::identifiers::{ResourceId, StringId};
use crate::table::v14::visual::{LODPrototype14, RenderSetPrototype14, SkeletonPrototype14, VisualPrototype14};
use crate::table::{BigWorldTableRecord, TableRecord};
use crate::{bigworld_table_check, bigworld_table_version};
use akizuki_macro::akizuki_id;

use std::collections::HashMap;
use std::io::Cursor;

// everything is an option because these are the sum of all versions.

#[derive(Debug)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub enum VisualPrototypeVersion {
	V14(VisualPrototype14),
}

#[derive(Debug)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub enum SkeletonPrototypeVersion {
	V14(SkeletonPrototype14),
}

#[derive(Debug)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub enum RenderSetPrototypeVersion {
	V14(RenderSetPrototype14),
}

#[derive(Debug)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub enum LODPrototypeVersion {
	V14(LODPrototype14),
}

impl TableRecord for VisualPrototypeVersion {
	fn new(reader: &mut Cursor<Vec<u8>>, header: &BigWorldTableHeader) -> AkizukiResult<Self> {
		if header.id != akizuki_id!("VisualPrototype") {
			return Err(AkizukiError::UnsupportedTable(header.id));
		}

		bigworld_table_check!(VisualPrototype14, VisualPrototypeVersion::V14, reader, header);

		Err(AkizukiError::UnsupportedTableVersion(header.id, header.version))
	}

	fn is_supported(header: &BigWorldTableHeader) -> bool {
		bigworld_table_version!(VisualPrototype14, header);
		false
	}
}

impl From<VisualPrototypeVersion> for BigWorldTableRecord {
	fn from(value: VisualPrototypeVersion) -> Self {
		BigWorldTableRecord::VisualPrototype(value)
	}
}

impl From<SkeletonPrototypeVersion> for BigWorldTableRecord {
	fn from(value: SkeletonPrototypeVersion) -> Self {
		BigWorldTableRecord::SkeletonPrototype(value)
	}
}

impl VisualPrototypeVersion {
	pub fn skeleton_prototype(&self) -> &SkeletonPrototypeVersion {
		match self {
			VisualPrototypeVersion::V14(v14) => &v14.skeleton_prototype,
		}
	}
	pub fn merged_geometry_path(&self) -> ResourceId {
		match self {
			VisualPrototypeVersion::V14(v14) => v14.merged_geometry_path,
		}
	}
	pub fn is_underwater_model(&self) -> bool {
		match self {
			VisualPrototypeVersion::V14(v14) => v14.is_underwater_model,
		}
	}
	pub fn is_abovewater_model(&self) -> bool {
		match self {
			VisualPrototypeVersion::V14(v14) => v14.is_abovewater_model,
		}
	}
	pub fn bounding_box(&self) -> &BoundingBox {
		match self {
			VisualPrototypeVersion::V14(v14) => &v14.bounding_box,
		}
	}
	pub fn render_sets(&self) -> &HashMap<StringId, RenderSetPrototypeVersion> {
		match self {
			VisualPrototypeVersion::V14(v14) => &v14.render_sets,
		}
	}
	pub fn lods(&self) -> &Vec<LODPrototypeVersion> {
		match self {
			VisualPrototypeVersion::V14(v14) => &v14.lods,
		}
	}
}

impl SkeletonPrototypeVersion {
	pub fn names(&self) -> &Vec<StringId> {
		match self {
			SkeletonPrototypeVersion::V14(v14) => &v14.names,
		}
	}

	pub fn matrices(&self) -> &Vec<Mat4> {
		match self {
			SkeletonPrototypeVersion::V14(v14) => &v14.matrices,
		}
	}

	pub fn parent_ids(&self) -> &Vec<u16> {
		match self {
			SkeletonPrototypeVersion::V14(v14) => &v14.parent_ids,
		}
	}
}

impl RenderSetPrototypeVersion {
	pub fn name(&self) -> StringId {
		match self {
			RenderSetPrototypeVersion::V14(v14) => v14.name,
		}
	}

	pub fn material_name(&self) -> StringId {
		match self {
			RenderSetPrototypeVersion::V14(v14) => v14.material_name,
		}
	}

	pub fn vertices_name(&self) -> StringId {
		match self {
			RenderSetPrototypeVersion::V14(v14) => v14.vertices_name,
		}
	}

	pub fn indices_name(&self) -> StringId {
		match self {
			RenderSetPrototypeVersion::V14(v14) => v14.indices_name,
		}
	}

	pub fn material_resource(&self) -> ResourceId {
		match self {
			RenderSetPrototypeVersion::V14(v14) => v14.material_resource,
		}
	}

	pub fn is_skinned(&self) -> bool {
		match self {
			RenderSetPrototypeVersion::V14(v14) => v14.is_skinned,
		}
	}

	pub fn nodes(&self) -> &Vec<StringId> {
		match self {
			RenderSetPrototypeVersion::V14(v14) => &v14.nodes,
		}
	}
}

impl LODPrototypeVersion {
	pub fn extent(&self) -> f32 {
		match self {
			LODPrototypeVersion::V14(v14) => v14.extent,
		}
	}

	pub fn cast_shadows(&self) -> bool {
		match self {
			LODPrototypeVersion::V14(v14) => v14.cast_shadows,
		}
	}

	pub fn render_sets(&self) -> &Vec<StringId> {
		match self {
			LODPrototypeVersion::V14(v14) => &v14.render_sets,
		}
	}
}
