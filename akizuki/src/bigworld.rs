// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::format::bigworld_data::BigWorldPrototypeRef;
use crate::identifiers::ResourceId;
use std::collections::HashMap;

pub struct BigWorldDatabase {
	pub name: String,
	pub prototype_lookup: HashMap<ResourceId, BigWorldPrototypeRef>,
}

impl BigWorldDatabase {
	pub(crate) fn new(_asset_bin: Vec<u8>, _validate: bool) -> Option<BigWorldDatabase> {
		todo!()
	}
}
