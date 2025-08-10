// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

#[cfg(feature = "data")]
pub mod bigworld;
#[cfg(any(feature = "data", feature = "geometry"))]
pub mod bin_wrap;
pub mod error;
pub mod format;
pub mod identifiers;
pub mod macros;
pub mod manager;
pub mod pfs;
#[cfg(feature = "geometry")]
pub mod space;
#[cfg(feature = "data")]
pub mod table;
