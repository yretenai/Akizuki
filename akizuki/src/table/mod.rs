// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

pub mod model;
pub mod v14;

use crate::error::AkizukiResult;
use crate::format::bigworld_data::BigWorldTableHeader;
use crate::table::model::ModelPrototype;

use std::io::Cursor;

pub enum BigWorldTableRecord {
	ModelPrototype(ModelPrototype),
}

#[allow(dead_code)]
pub(crate) trait TableRecord {
	fn new(reader: &mut Cursor<Vec<u8>>, header: &BigWorldTableHeader) -> AkizukiResult<Self>
	where
		Self: Sized;
	fn is_supported(header: &BigWorldTableHeader) -> bool;
}
