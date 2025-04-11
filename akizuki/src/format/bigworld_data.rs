// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::identifiers::{ResourceId, StringId};

use binrw::{BinRead, BinResult, Endian, PosValue};

use std::fmt;
use std::io::SeekFrom::Current;
use std::io::{Read, Seek};

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
	pub relative_position: PosValue<()>,

	pub strings: BigWorldDatabaseMap,
	pub string_data: BigWorldDatabasePointer,
	pub prototypes: BigWorldDatabaseMap,
	pub paths: BigWorldDatabasePointer,
	pub tables: BigWorldDatabasePointer,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct BigWorldTableHeader {
	pub relative_position: PosValue<()>,

	pub id: StringId,
	pub version: u32,
	pub pointer: BigWorldDatabasePointer,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct BigWorldName {
	pub id: ResourceId,
	pub parent_id: ResourceId,
	pub pointer: BigWorldDatabasePointer,
}

#[derive(Debug, Clone)]
pub struct BigWorldDatabaseKey<T: binrw::BinRead> {
	pub id: T,
	pub bucket: u32,
}

impl<T: BinRead> BinRead for BigWorldDatabaseKey<T> {
	type Args<'a> = T::Args<'a>;

	fn read_options<R: Read + Seek>(reader: &mut R, endian: Endian, args: Self::Args<'_>) -> BinResult<Self> {
		let id = T::read_options(reader, endian, args)?;
		if size_of::<T>() == 8 {
			reader.seek(Current(4))?;
		}
		let bucket = u32::read_options(reader, endian, ())?;

		Ok(BigWorldDatabaseKey::<T> { id, bucket })
	}
}

#[repr(C)]
#[derive(BinRead, Clone, Copy, PartialEq, Eq, Hash)]
#[br(repr = u32)]
pub struct BigWorldPrototypeRef(pub u32);

impl BigWorldPrototypeRef {
	// todo: 0 = normal, 3 = deleted. what is 1 and 2?
	pub fn state(&self) -> i32 {
		(self.0 & 3) as i32
	}

	pub fn table_index(&self) -> usize {
		((self.0 >> 2) & 0x3F) as usize
	}

	pub fn record_index(&self) -> usize {
		(self.0 >> 8) as usize
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

impl fmt::Debug for BigWorldPrototypeRef {
	fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
		write!(
			f,
			"ref {{ state = {:?}, table = {:?}, record = {:?} }}",
			self.state(),
			self.table_index(),
			self.record_index()
		)
	}
}
