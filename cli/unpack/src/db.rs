// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::{Cli, NEWLINE};
use akizuki::bigworld::BigWorldDatabase;
use akizuki::identifiers::ResourceId;
use akizuki::manager::ResourceManager;

use log::{error, info};

use std::fs;
use std::fs::File;
use std::io::{BufWriter, Write};
use std::path::Path;

pub fn process_db_records(args: &Cli, output_path: &Path, manager: &ResourceManager) -> anyhow::Result<()> {
	let db = manager.big_world_database.as_ref().expect("unreachable");

	for record_id in db.prototype_lookup.keys() {
		if let Err(err) = process_db_asset(args, output_path, db, *record_id) {
			error!(target: "akizuki::unpack", "unable to export data {:?}: {}", record_id, err);
		}
	}

	Ok(())
}

pub fn process_db_tables(args: &Cli, output_path: &Path, manager: &ResourceManager) -> anyhow::Result<()> {
	let db = manager.big_world_database.as_ref().expect("unreachable");
	let asset_bin = manager.load_asset(ResourceId::new("content/assets.bin"), false)?;

	for header in &db.table_headers {
		let asset_name = format!("{}_{:08x}.bin", header.id.text().unwrap_or_else(|| format!("{:08x}", header.id.value())), header.version);

		let data = db.load_table_slice(&asset_bin, header);

		info!(target: "akizuki::unpack", "Saving table {:?}", header.id);

		if args.dry {
			return Ok(());
		}

		let asset_path = output_path.join("tables").join(asset_name);
		let asset_dir = asset_path.parent().unwrap_or(output_path);

		fs::create_dir_all(asset_dir)?;
		fs::write(&asset_path, data)?;
	}

	Ok(())
}

pub fn process_db_asset(args: &Cli, output_path: &Path, db: &BigWorldDatabase, record_id: ResourceId) -> anyhow::Result<()> {
	let record = db.open(record_id)?;
	let asset_name = record_id.text().unwrap_or_else(|| format!("unknown/{:016x}", record_id.value()));

	if !args.filter.is_empty() && !args.filter.iter().any(|v| asset_name.contains(v)) {
		return Ok(());
	}

	info!(target: "akizuki::unpack", "Saving {:?}", record_id);

	if args.dry {
		return Ok(());
	}

	let asset_path = output_path.join(asset_name + ".json");
	let asset_dir = asset_path.parent().unwrap_or(output_path);

	fs::create_dir_all(asset_dir)?;
	let file = File::create(asset_path)?;
	file.set_len(0)?;
	let mut writer = BufWriter::new(file);
	serde_json::to_writer_pretty(&mut writer, record)?;
	writer.write_all(&NEWLINE)?;
	Ok(())
}
