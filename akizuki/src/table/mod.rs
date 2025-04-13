// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

pub mod v14;

pub mod material;
pub mod model;
pub mod visual;

use crate::error::AkizukiResult;
use crate::format::bigworld_data::BigWorldTableHeader;

use std::io::Cursor;

#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
#[serde(tag = "table")]
pub enum BigWorldTableRecord {
	VisualPrototype(Box<visual::VisualPrototypeVersion>),
	SkeletonPrototype(Box<visual::SkeletonPrototypeVersion>),
	ModelPrototype(Box<model::ModelPrototypeVersion>),
	MaterialPrototype(Box<material::MaterialPrototypeVersion>),
}

#[allow(dead_code)]
pub(crate) trait TableRecord {
	fn new(reader: &mut Cursor<Vec<u8>>, header: &BigWorldTableHeader) -> AkizukiResult<Self>
	where
		Self: Sized;
	fn is_supported(header: &BigWorldTableHeader) -> bool;
}
