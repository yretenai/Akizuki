// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use std::collections::HashMap;
use std::io::Cursor;

use akizuki_macro::akizuki_id;

use crate::error::{AkizukiError, AkizukiResult};
use crate::format::bigworld_data::BigWorldTableHeader;
use crate::format::bigworld_table::ModelMiscType;
use crate::identifiers::{ResourceId, StringId};
use crate::table::model_proto::v14_1_0::*;
use crate::table::{BigWorldTableRecord, TableRecord};
use crate::{bigworld_table_check, bigworld_table_version};

#[derive(Debug)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
#[serde(tag = "version")]
pub enum ModelPrototypeVersion {
	V14_1_0(ModelPrototype14_1_0),
}

#[derive(Debug)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
#[serde(tag = "version")]
pub enum DyePrototypeVersion {
	V14_1_0(DyePrototype14_1_0),
}

impl TableRecord for ModelPrototypeVersion {
	fn new(reader: &mut Cursor<Vec<u8>>, header: &BigWorldTableHeader) -> AkizukiResult<Self> {
		if header.id != akizuki_id!("ModelPrototype") {
			return Err(AkizukiError::UnsupportedTable(header.id));
		}

		bigworld_table_check!(ModelPrototype14_1_0, ModelPrototypeVersion::V14_1_0, reader, header);

		Err(AkizukiError::UnsupportedTableVersion(header.id, header.version))
	}

	fn is_supported(header: &BigWorldTableHeader) -> bool {
		bigworld_table_version!(ModelPrototype14_1_0, header);
		false
	}
}

impl From<ModelPrototypeVersion> for BigWorldTableRecord {
	fn from(value: ModelPrototypeVersion) -> Self {
		BigWorldTableRecord::ModelPrototype(value.into())
	}
}

impl ModelPrototypeVersion {
	pub fn visual_resource(&self) -> ResourceId {
		match self {
			ModelPrototypeVersion::V14_1_0(v14) => v14.visual_resource,
		}
	}
	pub fn misc_type(&self) -> ModelMiscType {
		match self {
			ModelPrototypeVersion::V14_1_0(v14) => v14.misc_type,
		}
	}

	pub fn animations(&self) -> &Vec<ResourceId> {
		match self {
			ModelPrototypeVersion::V14_1_0(v14) => &v14.animations,
		}
	}

	pub fn dyes(&self) -> &Vec<DyePrototypeVersion> {
		match self {
			ModelPrototypeVersion::V14_1_0(v14) => &v14.dyes,
		}
	}
}

impl DyePrototypeVersion {
	pub fn matter(&self) -> StringId {
		match self {
			DyePrototypeVersion::V14_1_0(v14) => v14.matter,
		}
	}

	pub fn replaces(&self) -> StringId {
		match self {
			DyePrototypeVersion::V14_1_0(v14) => v14.replaces,
		}
	}

	pub fn tints(&self) -> &HashMap<StringId, ResourceId> {
		match self {
			DyePrototypeVersion::V14_1_0(v14) => &v14.tints,
		}
	}
}
