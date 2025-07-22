// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

pub mod material_proto;
pub mod model_proto;
pub mod skeleton_proto;
pub mod visual_proto;

pub mod material;
pub mod model;
pub mod skeleton;
pub mod visual;

use std::io::Cursor;

use crate::error::AkizukiResult;
use crate::format::bigworld_data::BigWorldTableHeader;

#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
#[serde(tag = "table")]
pub enum BigWorldTableRecord {
	VisualPrototype(Box<visual::VisualPrototypeVersion>),
	SkeletonPrototype(Box<skeleton::SkeletonPrototypeVersion>),
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
