// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::format::oodle::OodleError;
use crate::identifiers::{ResourceId, StringId};

use thiserror::Error;

#[non_exhaustive]
#[derive(Error, Debug)]
pub enum AkizukiError {
	#[error("install path is invalid")]
	InvalidInstall,
	#[error("version mismatch, expected {expected:08x} got {present:08x}")]
	InvalidVersion { expected: u32, present: u32 },
	#[error("identifier mismatch")]
	InvalidIdentifier,
	#[error("pointer size is not 64-bit")]
	InvalidPointerSize,
	#[error("asset failed checksum verification")]
	ChecksumMismatch,
	#[error("asset {0:?} is not found")]
	AssetNotFound(ResourceId),
	#[error("asset {0:?} is deleted")]
	DeletedAsset(ResourceId),
	#[error("asset {0:?} is referencing an invalid table")]
	InvalidTable(ResourceId),
	#[error("asset {0:?} is referencing an invalid record")]
	InvalidRecord(ResourceId),
	#[error("table {0:?} is not supported")]
	UnsupportedTable(StringId),
	#[error("table {0:?} is has an unsupported version {1:08x}")]
	UnsupportedTableVersion(StringId, u32),

	#[error("io error: {0}")]
	Std(#[from] std::io::Error),
	#[error("binrw error: {0}")]
	Bin(#[from] binrw::Error),
	#[error("zlib error: {0}")]
	Flate(#[from] flate2::DecompressError),
	#[error("oodle error: {0}")]
	Oodle(#[from] OodleError),
}

pub type AkizukiResult<T> = Result<T, AkizukiError>;
