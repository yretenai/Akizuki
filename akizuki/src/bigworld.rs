// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::error::{AkizukiError, AkizukiResult};
use crate::format::bigworld::{BigWorldFileHeader, BigWorldMagic};
use crate::format::bigworld_data::{BigWorldDatabaseHeader, BigWorldDatabaseKey, BigWorldName, BigWorldPrototypeRef};
use crate::identifiers::{ResourceId, StringId};
use crate::pfs;
use crate::table::BigWorldTableRecord;

use binrw::{BinRead, NullString, VecArgs};
use log::info;

use std::collections::HashMap;
use std::io::SeekFrom::Start;
use std::io::{Cursor, Seek};

type Table = Vec<BigWorldTableRecord>;
type TableState = Option<StringId>;

pub struct BigWorldDatabase {
	pub prototype_lookup: HashMap<ResourceId, BigWorldPrototypeRef>,
	pub tables: Vec<Table>,
	pub table_state: Vec<TableState>,
}

impl BigWorldDatabase {
	pub fn new(asset_bin: Vec<u8>, validate: bool) -> AkizukiResult<BigWorldDatabase> {
		info!("loading asset db");

		let mut reader = Cursor::new(asset_bin);
		let bw_header = BigWorldFileHeader::read_ne(&mut reader)?;
		bw_header.is_valid(BigWorldMagic::AssetDb, 257, validate, &mut reader)?;

		let bwdb_header = BigWorldDatabaseHeader::read_ne(&mut reader)?;

		read_strings(&mut reader, &bwdb_header)?;
		let names = read_names(&mut reader, &bwdb_header)?;
		let prototype_lookup = read_prototype_lookup(&mut reader, &bwdb_header)?;
		pfs::build_filenames(&names, &prototype_lookup);
		let (tables, table_state) = read_tables(&mut reader, &bwdb_header)?;

		Ok(BigWorldDatabase {
			prototype_lookup,
			tables,
			table_state,
		})
	}

	pub fn open(&self, id: &ResourceId) -> AkizukiResult<&BigWorldTableRecord> {
		let info = self.prototype_lookup.get(id).ok_or(AkizukiError::AssetNotFound(*id))?;
		if !info.is_valid() {
			return Err(AkizukiError::DeletedAsset(*id));
		}

		let table_index = info.table_index();

		// check if the table is valid and supported
		self.table_state
			.get(table_index)
			.ok_or(AkizukiError::InvalidTable(*id))?
			.as_ref()
			.map(|table_state| Err(AkizukiError::UnsupportedTable(*table_state)))
			.unwrap_or(Ok(()))?;

		// return the record if it exists
		self.tables
			.get(table_index)
			.ok_or(AkizukiError::InvalidTable(*id))?
			.get(info.record_index())
			.ok_or(AkizukiError::InvalidRecord(*id))
	}
}

fn read_tables(
	_reader: &mut Cursor<Vec<u8>>,
	_header: &BigWorldDatabaseHeader,
) -> AkizukiResult<(Vec<Table>, Vec<TableState>)> {
	todo!()
}

fn read_strings(reader: &mut Cursor<Vec<u8>>, header: &BigWorldDatabaseHeader) -> AkizukiResult<()> {
	let base = header.relative_position.pos + header.string_data.offset;

	reader.seek(Start(header.relative_position.pos + header.strings.key_offset))?;
	let keys = Vec::<BigWorldDatabaseKey<StringId>>::read_ne_args(
		reader,
		VecArgs {
			count: header.strings.count as usize,
			inner: <_>::default(),
		},
	)?;

	reader.seek(Start(header.relative_position.pos + header.strings.value_offset))?;
	let values = Vec::<u32>::read_ne_args(
		reader,
		VecArgs {
			count: header.strings.count as usize,
			inner: <_>::default(),
		},
	)?;

	for (key, offset) in keys.iter().zip(values) {
		if (key.bucket & 0x80000000) == 0 {
			continue;
		}

		reader.seek(Start(base + offset as u64))?;
		let string = NullString::read_ne(reader)?.to_string();
		StringId::insert(&key.id, &string);
	}

	Ok(())
}

fn read_names(
	reader: &mut Cursor<Vec<u8>>,
	header: &BigWorldDatabaseHeader,
) -> AkizukiResult<HashMap<ResourceId, (String, ResourceId)>> {
	reader.seek(Start(header.paths.relative_position.pos + header.paths.offset))?;

	let names = Vec::<BigWorldName>::read_ne_args(
		reader,
		VecArgs {
			count: header.paths.count as usize,
			inner: <_>::default(),
		},
	)?;
	let mut name_map = HashMap::<ResourceId, (String, ResourceId)>::new();
	for name in names {
		reader.seek(Start(name.pointer.relative_position.pos + name.pointer.offset))?;
		name_map.insert(name.id, (NullString::read_ne(reader)?.to_string(), name.parent_id));
	}

	Ok(name_map)
}

fn read_prototype_lookup(
	reader: &mut Cursor<Vec<u8>>,
	header: &BigWorldDatabaseHeader,
) -> AkizukiResult<HashMap<ResourceId, BigWorldPrototypeRef>> {
	reader.seek(Start(
		header.prototypes.relative_position.pos + header.prototypes.key_offset,
	))?;
	let keys = Vec::<BigWorldDatabaseKey<ResourceId>>::read_ne_args(
		reader,
		VecArgs {
			count: header.prototypes.count as usize,
			inner: <_>::default(),
		},
	)?;

	reader.seek(Start(
		header.prototypes.relative_position.pos + header.prototypes.value_offset,
	))?;
	let values = Vec::<BigWorldPrototypeRef>::read_ne_args(
		reader,
		VecArgs {
			count: header.prototypes.count as usize,
			inner: <_>::default(),
		},
	)?;

	let mut prototype_map = HashMap::<ResourceId, BigWorldPrototypeRef>::new();
	for (key, value) in keys.iter().zip(values) {
		if (key.bucket & 0x80000000) == 0 {
			continue;
		}

		if !value.is_valid() {
			continue;
		}

		prototype_map.insert(key.id, value);
	}

	Ok(prototype_map)
}
