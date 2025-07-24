// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use std::fmt::{Debug, Display, Formatter};
use std::io::SeekFrom::{End, Start};
use std::io::{Read, Seek};

use akizuki_common::mmh3::mmh3_32;
use binrw::BinRead;
use four_char_code::four_char_code;
use log::debug;

use crate::error::{AkizukiError, AkizukiResult};

#[derive(BinRead, Debug, Clone, Copy, PartialEq, Eq, Hash)]
#[br(repr = u32)]
pub enum BigWorldMagic {
	PFSIndex = four_char_code!("PFSI").as_u32() as isize,
	AssetDb = four_char_code!("BWDB").as_u32() as isize,
}

#[derive(BinRead, Debug, Clone, Copy, PartialEq, Eq, Hash)]
#[br()]
pub struct BigWorldFileVersion {
	pub revision: u8,
	pub patch: u8,
	pub minor: u8,
	pub major: u8,
}

#[derive(BinRead, Debug, Clone, Copy, PartialEq, Eq, Hash)]
#[br()]
pub struct BigWorldFileHeader {
	pub magic: BigWorldMagic,
	pub version: BigWorldFileVersion,
	pub hash: u32,
	pub pointer_size: u32,
}


impl BigWorldFileVersion {
	pub fn new(major: i32, minor: i32, patch: i32, revision: i32) -> Self {
		Self {
			major: major as u8,
			minor: minor as u8,
			patch: patch as u8,
			revision: revision as u8,
		}
	}
}

impl Display for BigWorldFileVersion {
	fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
		write!(f, "{:}.{:}.{:}.{:}", self.major, self.minor, self.patch, self.revision)
	}
}

impl BigWorldFileHeader {
	pub(crate) fn is_valid<T: Read + Seek>(&self, magic: BigWorldMagic, version: BigWorldFileVersion, validate: bool, reader: &mut T) -> AkizukiResult<()> {
		if self.version != version {
			return Err(AkizukiError::InvalidVersion {
				expected: version,
				present: self.version,
			});
		}

		if self.magic != magic {
			return Err(AkizukiError::InvalidIdentifier);
		}

		if self.pointer_size != 64 {
			return Err(AkizukiError::InvalidPointerSize);
		}

		if validate {
			reader.seek(End(0))?;
			let capacity = reader.stream_position()? - 0x10;

			reader.seek(Start(0x10))?;
			let mut all_data = Vec::<u8>::with_capacity(capacity as usize);
			reader.read_to_end(&mut all_data)?;

			let hash = mmh3_32(&all_data);
			if hash != self.hash {
				return Err(AkizukiError::ChecksumMismatch);
			}

			reader.seek(Start(0x10))?;

			debug!("big world header passed validation");
		}

		Ok(())
	}
}
