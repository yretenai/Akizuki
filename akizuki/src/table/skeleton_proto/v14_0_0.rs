// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use std::collections::HashMap;
use std::io::SeekFrom::Start;
use std::io::{Cursor, Seek};

use akizuki_macro::BigWorldTable;
use binrw::{BinRead, BinReaderExt, PosValue, VecArgs};

use crate::bigworld_read_array;
use crate::bin_wrap::Mat4;
use crate::error::AkizukiResult;
use crate::identifiers::StringId;

#[derive(BinRead, Debug)]
#[br()]
pub struct SkeletonPrototypeHeader14_0_0 {
	pub relative_position: PosValue<()>,

	pub node_count: u32,
	#[br(pad_before = 4)]
	pub name_map_id_offset: u64,
	pub name_map_node_offset: u64,
	pub name_ids_offset: u64,
	pub matrices_offset: u64,
	pub parent_ids_offset: u64,

	pub end_position: PosValue<()>,
}

#[derive(BigWorldTable, Debug)]
#[table("SkeletonPrototype", 0x459958ae)]
#[cfg_attr(feature = "serialize", derive(serde::Serialize), serde_with::serde_as)]
pub struct SkeletonPrototype14_0_0 {
	pub node_map: HashMap<StringId, u16>,
	pub names: Vec<StringId>,
	pub matrices: Vec<Mat4>,
	pub parent_ids: Vec<u16>,
}

impl SkeletonPrototype14_0_0 {
	pub fn new(reader: &mut Cursor<Vec<u8>>) -> AkizukiResult<Self> {
		let header = reader.read_ne::<SkeletonPrototypeHeader14_0_0>()?;
		bigworld_read_array!(reader, header, name_map_ids, node_count, name_map_id_offset, StringId);
		bigworld_read_array!(reader, header, name_map_nodes, node_count, name_map_node_offset, u16);
		bigworld_read_array!(reader, header, names, node_count, name_ids_offset, StringId);
		bigworld_read_array!(reader, header, matrices, node_count, matrices_offset, Mat4);
		bigworld_read_array!(reader, header, parent_ids, node_count, parent_ids_offset, u16);

		Ok(SkeletonPrototype14_0_0 {
			node_map: name_map_ids.iter().cloned().zip(name_map_nodes).collect(),
			names,
			matrices,
			parent_ids,
		})
	}
}
