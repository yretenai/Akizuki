// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use thiserror::Error;

use crate::format::bigworld::BigWorldFileVersion;
use crate::format::oodle::OodleError;
use crate::identifiers::{ResourceId, StringId};

#[non_exhaustive]
#[derive(Error, Debug)]
pub enum AkizukiError {
	#[error("install path is invalid")]
	InvalidInstall,
	#[error("version mismatch, expected {expected} got {present}")]
	InvalidVersion { expected: BigWorldFileVersion, present: BigWorldFileVersion },
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
	#[error("resource {0:?} has an unrecognized vertex format {1}")]
	UnrecognizedVertexFormat(ResourceId, String),
	#[error("resource {0:?} has an unrecognized vertex index size {1}")]
	UnrecognizedIndexFormat(ResourceId, u16),

	#[error("io error: {0}")]
	Std(#[from] std::io::Error),
	#[error("binrw error: {0}")]
	Bin(#[from] binrw::Error),
	#[error("zlib error: {0}")]
	Flate(#[from] flate2::DecompressError),
	#[error("oodle error: {0}")]
	Oodle(#[from] OodleError),
	#[error("meshopt error: {0}")]
	MeshOpt(#[from] meshopt::Error),
	#[error("cast error: {0}")]
	PodCast(bytemuck::PodCastError),
}

pub type AkizukiResult<T> = Result<T, AkizukiError>;
