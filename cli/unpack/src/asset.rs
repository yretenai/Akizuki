// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::Cli;
use akizuki::identifiers::ResourceId;
use akizuki::pfs::PackageFileSystem;
use log::info;
use std::fs;
use std::path::Path;

pub fn process_asset(args: &Cli, output_path: &Path, package: &PackageFileSystem, asset_id: ResourceId) -> anyhow::Result<()> {
	let asset_name = asset_id.text().unwrap_or_else(|| format!("unknown/{:016x}", asset_id.value()));

	if !args.filter.is_empty() && !args.filter.iter().any(|v| asset_name.contains(v)) {
		return Ok(());
	}

	let data = package.open(asset_id, args.validate)?;

	info!(target: "akizuki::unpack", "Unpacking {:?}", asset_id);

	if args.dry {
		return Ok(());
	}

	let asset_path = output_path.join(asset_name);
	let asset_dir = asset_path.parent().unwrap_or(output_path);

	fs::create_dir_all(asset_dir)?;
	fs::write(&asset_path, data)?;
	Ok(())
}
