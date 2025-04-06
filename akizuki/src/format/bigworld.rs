// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::identifiers::mmh3_32;

use binrw::io::BufReader;
use binrw::{BinRead, BinResult};
use four_char_code::four_char_code;
use log::debug;

use std::fs::File;
use std::io;
use std::io::SeekFrom::Start;
use std::io::{ErrorKind, Read, Seek};

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
	pub(crate) fn validate(&self, magic: BigWorldMagic, version: u32, validate: bool, reader: &mut BufReader<File>) -> BinResult<()> {
		let swapped_version = u32::swap_bytes(self.version_be);

		if swapped_version > self.version_be {
			return Err(io::Error::new(ErrorKind::InvalidData, "endian mismatch").into());
		}

		if swapped_version != version {
			return Err(io::Error::new(ErrorKind::InvalidData, "unsupported version").into());
		}

		if self.magic != magic {
			return Err(io::Error::new(ErrorKind::InvalidData, "no versions present").into());
		}

		if self.pointer_size != 64 {
			return Err(io::Error::new(ErrorKind::InvalidData, "unsupported pointer size").into());
		}

		if validate {
			let mut all_data = Vec::<u8>::with_capacity(reader.capacity() - 0x10);
			reader.read_to_end(&mut all_data)?;

			let hash = mmh3_32(all_data);
			if hash != self.hash {
				return Err(io::Error::new(ErrorKind::InvalidData, "checksum mismatch").into());
			}

			reader.seek(Start(0x10))?;
		}

		debug!("big world header passed validation");

		Ok(())
	}
}
