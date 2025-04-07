// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::identifiers::{ResourceId, StringId, mmh3_32};

use binrw::io::BufReader;
use binrw::{BinRead, BinResult, PosValue};
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

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct BigWorldDatabaseMap {
	pub relative_position: PosValue<()>,

	pub count: u64,
	pub key_offset: u64,
	pub value_offset: u64,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct BigWorldDatabasePointer {
	pub relative_position: PosValue<()>,

	pub count: u64,
	pub offset: u64,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct BigWorldDatabaseHeader {
	pub strings: BigWorldDatabaseMap,
	pub string_data: BigWorldDatabasePointer,
	pub prototypes: BigWorldDatabaseMap,
	pub paths: BigWorldDatabasePointer,
	pub tables: BigWorldDatabasePointer,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct BigWorldTableHeader {
	pub id: StringId,
	pub hash: u32,
	pub pointer: BigWorldDatabasePointer,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct BigWorldName {
	pub id: ResourceId,
	pub parent_id: ResourceId,
	pub pointer: BigWorldDatabasePointer,
}

#[repr(C)]
#[derive(BinRead, Clone, Copy, PartialEq, Eq, Hash)]
#[br(repr = u32)]
pub struct BigWorldPrototypeRef(pub u32);

impl BigWorldFileHeader {
	pub(crate) fn is_valid(&self, magic: BigWorldMagic, version: u32, validate: bool, reader: &mut BufReader<File>) -> BinResult<()> {
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

			debug!("big world header passed validation");
		}

		Ok(())
	}
}

impl BigWorldPrototypeRef {
	pub fn state(&self) -> i32 {
		(self.0 & 3) as i32
	}

	pub fn table_index(&self) -> i32 {
		((self.0 >> 2) & 0x3F) as i32
	}

	pub fn record_index(&self) -> i32 {
		(self.0 >> 8) as i32
	}

	pub fn is_valid(&self) -> bool {
		self.state() == 0
	}
}

impl From<u32> for BigWorldPrototypeRef {
	fn from(value: u32) -> Self {
		Self(value)
	}
}
