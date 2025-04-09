// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::format::oodle::OodleError;

use thiserror::Error;

#[non_exhaustive]
#[derive(Error, Debug)]
pub enum AkizukiError {
	#[error("install path is invalid")]
	InvalidInstall,
	#[error("file endianness does not match host endianness")]
	InvalidEndianness,
	#[error("version mismatch, expected {expected:?} got {present:?}")]
	InvalidVersion { expected: u32, present: u32 },
	#[error("identifier mismatch")]
	InvalidIdentifier,
	#[error("pointer size is not 64-bit")]
	InvalidPointerSize,
	#[error("asset failed checksum verification")]
	ChecksumMismatch,

	#[error("io error: {0}")]
	Std(#[from] std::io::Error),
	#[error("binrw error: {0}")]
	Bin(#[from] binrw::Error),
	#[error("decompression error: {0}")]
	Flate(#[from] flate2::DecompressError),
	#[error("oodle error: {0}")]
	Oodle(#[from] OodleError),
}

pub type AkizukiResult<T> = Result<T, AkizukiError>;
