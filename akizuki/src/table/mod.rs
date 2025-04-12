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
#[serde(tag = "table")]
pub enum BigWorldTableRecord {
	VisualPrototype(visual::VisualPrototypeVersion),
	SkeletonPrototype(visual::SkeletonPrototypeVersion),
	ModelPrototype(model::ModelPrototypeVersion),
}

#[macro_export]
macro_rules! bigworld_table_check {
	($name:ident, $version:path, $reader:ident, $header:ident) => {
		if $name::is_valid_for($header.id, $header.version) {
			return Ok($version($name::new($reader)?.into()));
		}
	};
}

#[macro_export]
macro_rules! bigworld_table_version {
	($name:ident, $header:ident) => {
		if $name::is_valid_for($header.id, $header.version) {
			return true;
		}
	};
}

#[allow(dead_code)]
pub(crate) trait TableRecord {
	fn new(reader: &mut Cursor<Vec<u8>>, header: &BigWorldTableHeader) -> AkizukiResult<Self>
	where
		Self: Sized;
	fn is_supported(header: &BigWorldTableHeader) -> bool;
}
