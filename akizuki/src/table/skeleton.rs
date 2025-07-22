// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use std::collections::HashMap;
use std::io::Cursor;

use akizuki_macro::akizuki_id;

use crate::bin_wrap::Mat4;
use crate::error::{AkizukiError, AkizukiResult};
use crate::format::bigworld_data::BigWorldTableHeader;
use crate::identifiers::StringId;
use crate::table::skeleton_proto::v14_0_0::SkeletonPrototype14_0_0;
use crate::table::{BigWorldTableRecord, TableRecord};
use crate::{bigworld_table_check, bigworld_table_version};

#[derive(Debug)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
#[serde(tag = "version")]
pub enum SkeletonPrototypeVersion {
	V14_0_0(SkeletonPrototype14_0_0),
}

impl TableRecord for SkeletonPrototypeVersion {
	fn new(reader: &mut Cursor<Vec<u8>>, header: &BigWorldTableHeader) -> AkizukiResult<Self> {
		if header.id != akizuki_id!("SkeletonPrototype") {
			return Err(AkizukiError::UnsupportedTable(header.id));
		}

		bigworld_table_check!(SkeletonPrototype14_0_0, SkeletonPrototypeVersion::V14_0_0, reader, header);

		Err(AkizukiError::UnsupportedTableVersion(header.id, header.version))
	}

	fn is_supported(header: &BigWorldTableHeader) -> bool {
		bigworld_table_version!(SkeletonPrototype14_0_0, header);
		false
	}
}

impl From<SkeletonPrototypeVersion> for BigWorldTableRecord {
	fn from(value: SkeletonPrototypeVersion) -> Self {
		BigWorldTableRecord::SkeletonPrototype(value.into())
	}
}

impl SkeletonPrototypeVersion {
	pub fn node_map(&self) -> &HashMap<StringId, u16> {
		match self {
			SkeletonPrototypeVersion::V14_0_0(v14) => &v14.node_map,
		}
	}

	pub fn names(&self) -> &Vec<StringId> {
		match self {
			SkeletonPrototypeVersion::V14_0_0(v14) => &v14.names,
		}
	}

	pub fn matrices(&self) -> &Vec<Mat4> {
		match self {
			SkeletonPrototypeVersion::V14_0_0(v14) => &v14.matrices,
		}
	}

	pub fn parent_ids(&self) -> &Vec<u16> {
		match self {
			SkeletonPrototypeVersion::V14_0_0(v14) => &v14.parent_ids,
		}
	}
}
