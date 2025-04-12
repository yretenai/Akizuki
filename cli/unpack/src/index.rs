// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::NEWLINE;
use akizuki::identifiers::{ResourceId, StringId};

use anyhow::Error;

use std::collections::HashMap;
use std::fs;
use std::fs::File;
use std::io::{BufWriter, Write};
use std::path::{Path, PathBuf};

pub fn process_index(output_path: &Path) -> anyhow::Result<()> {
	let asset_dir = &output_path.join("idx/");
	fs::create_dir_all(asset_dir)?;

	save_index(asset_dir.join("resource.json"), ResourceId::clone_map())?;
	save_index(asset_dir.join("string.json"), StringId::clone_map())?;

	Ok(())
}

pub fn save_index<T: serde::ser::Serialize>(
	asset_path: PathBuf,
	map: Option<HashMap<T, String>>,
) -> anyhow::Result<()> {
	let map = map.ok_or(Error::msg("no id data?"))?;

	if map.is_empty() {
		return Ok(());
	}

	let file = File::create(asset_path)?;
	file.set_len(0)?;
	let mut writer = BufWriter::new(file);
	serde_json::to_writer_pretty(&mut writer, &map)?;
	writer.write_all(&NEWLINE)?;
	Ok(())
}
