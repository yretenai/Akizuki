// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

pub mod v14;

pub mod model;
pub mod visual;

use crate::error::AkizukiResult;
use crate::format::bigworld_data::BigWorldTableHeader;

use std::io::Cursor;

#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub enum BigWorldTableRecord {
	VisualPrototype(visual::VisualPrototype),
	SkeletonPrototype(visual::SkeletonPrototype),
	ModelPrototype(model::ModelPrototype),
}

#[allow(dead_code)]
pub(crate) trait TableRecord {
	fn new(reader: &mut Cursor<Vec<u8>>, header: &BigWorldTableHeader) -> AkizukiResult<Self>
	where
		Self: Sized;
	fn is_supported(header: &BigWorldTableHeader) -> bool;
}
