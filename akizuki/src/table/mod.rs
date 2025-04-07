// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::table::model::ModelPrototype;

pub mod model;

pub enum BigWorldTableRecord {
	Model(ModelPrototype),
}
