// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::identifiers::{ResourceId, StringId};

use binrw::{BinRead, PosValue};

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
