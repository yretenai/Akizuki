// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::error::{AkizukiError, AkizukiResult};
use crate::format::bigworld_data::BigWorldTableHeader;
use crate::format::bigworld_table::ModelMiscType;
use crate::identifiers::{ResourceId, StringId};
use crate::table::{BigWorldTableRecord, TableRecord, v14};
use akizuki_macro::{akizuki_id, bigworld_table_check, bigworld_table_version};

use std::collections::HashMap;
use std::io::Cursor;

#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub struct ModelPrototype {
	pub visual_resource: Option<ResourceId>,
	pub misc_type: Option<ModelMiscType>,
	pub animations: Option<Vec<ResourceId>>,
	pub dyes: Option<Vec<DyePrototype>>,
}

#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub struct DyePrototype {
	pub matter: Option<StringId>,
	pub replaces: Option<StringId>,
	pub tints: Option<HashMap<StringId, ResourceId>>,
}

impl TableRecord for ModelPrototype {
	fn new(reader: &mut Cursor<Vec<u8>>, header: &BigWorldTableHeader) -> AkizukiResult<Self> {
		if header.id != akizuki_id!("ModelPrototype") {
			return Err(AkizukiError::UnsupportedTable(header.id));
		}

		bigworld_table_version!(v14::model::ModelPrototype14, reader, header);

		Err(AkizukiError::UnsupportedTableVersion(header.id, header.version))
	}

	fn is_supported(header: &BigWorldTableHeader) -> bool {
		bigworld_table_check!(v14::model::ModelPrototype14, header);
		false
	}
}

impl From<ModelPrototype> for BigWorldTableRecord {
	fn from(value: ModelPrototype) -> Self {
		BigWorldTableRecord::ModelPrototype(value)
	}
}
