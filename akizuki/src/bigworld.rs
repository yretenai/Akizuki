// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::format::bigworld::BigWorldPrototypeRef;
use crate::identifiers::ResourceId;
use std::collections::HashMap;

pub struct BigWorldDatabase {
	pub name: String,
	pub prototype_lookup: HashMap<ResourceId, BigWorldPrototypeRef>,
}
