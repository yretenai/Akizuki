// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::error::{AkizukiError, AkizukiResult};
use crate::identifiers::mmh3_32;

use binrw::BinRead;
use binrw::io::BufReader;
use four_char_code::four_char_code;
use log::debug;

use std::fs::File;
use std::io::SeekFrom::Start;
use std::io::{Read, Seek};

#[derive(BinRead, Debug, Clone, Copy, PartialEq, Eq, Hash)]
#[br(repr = u32)]
pub enum BigWorldMagic {
	PFSIndex = four_char_code!("PFSI").as_u32() as isize,
	AssetDb = four_char_code!("BWDB").as_u32() as isize,
}

#[derive(BinRead, Debug, Clone, Copy, PartialEq, Eq, Hash)]
#[br()]
pub struct BigWorldFileHeader {
	pub magic: BigWorldMagic,
	pub version_be: u32,
	pub hash: u32,
	pub pointer_size: u32,
}

impl BigWorldFileHeader {
	pub(crate) fn is_valid(&self, magic: BigWorldMagic, version: u32, validate: bool, reader: &mut BufReader<File>) -> AkizukiResult<()> {
		let swapped_version = u32::swap_bytes(self.version_be);

		if swapped_version > self.version_be {
			return Err(AkizukiError::InvalidEndianness);
		}

		if swapped_version != version {
			return Err(AkizukiError::InvalidVersion { expected: version, present: swapped_version });
		}

		if self.magic != magic {
			return Err(AkizukiError::InvalidIdentifier);
		}

		if self.pointer_size != 64 {
			return Err(AkizukiError::InvalidPointerSize);
		}

		if validate {
			let mut all_data = Vec::<u8>::with_capacity(reader.capacity() - 0x10);
			reader.read_to_end(&mut all_data)?;

			let hash = mmh3_32(all_data);
			if hash != self.hash {
				return Err(AkizukiError::ChecksumMismatch);
			}

			reader.seek(Start(0x10))?;

			debug!("big world header passed validation");
		}

		Ok(())
	}
}
