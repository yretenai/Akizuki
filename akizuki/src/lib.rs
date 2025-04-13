// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

pub mod format;
pub mod identifiers;

#[cfg(feature = "data")]
pub mod bigworld;
#[cfg(any(feature = "data", feature = "geometry"))]
pub mod bin_wrap;
pub mod error;
pub mod manager;
pub mod pfs;
#[cfg(feature = "data")]
pub mod table;

mod macros;
