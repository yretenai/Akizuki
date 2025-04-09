// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::error::AkizukiResult;
use crate::identifiers::StringId;
use crate::table::model::ModelPrototype;

use std::io::Cursor;

pub mod model;

pub enum BigWorldTableRecord {
	ModelPrototype(ModelPrototype),
}

#[allow(dead_code)]
pub(crate) trait TableRecord {
	fn is_valid_for(hash: &StringId, version: u32) -> bool;
	fn create(reader: &mut Cursor<&[u8]>) -> AkizukiResult<BigWorldTableRecord>;
}
