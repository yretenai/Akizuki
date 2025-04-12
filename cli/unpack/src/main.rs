// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use akizuki::bigworld::BigWorldDatabase;
use akizuki::error::AkizukiResult;
use akizuki::identifiers::{ResourceId, StringId};
use akizuki::manager::ResourceManager;
use akizuki::pfs::PackageFileSystem;

use anyhow::{Error, Result};
use clap::Parser;
use colog::format::CologStyle;
use colored::Colorize;
use env_logger::fmt::Formatter;
use log::{LevelFilter, Record, error, info};

use std::collections::HashMap;
use std::fs;
use std::fs::File;
use std::io::{BufWriter, Write};
use std::path::{Path, PathBuf};

const NEWLINE: [u8; 1] = [0xA];

#[derive(Parser)]
#[command(version, about)]
struct Cli {
	#[arg(index = 1, required = true, help = "path to write files")]
	output_path: String,

	#[arg(index = 2, required = true, help = "path to the game installation directory")]
	install_path: String,

	#[arg(
		index = 3,
		help = "version number of the game, if not set will try to find the latest version"
	)]
	install_version: Option<i64>,

	#[arg(long, help = "save asset indexes as JSON")]
	save_index: bool,

	#[arg(long, help = "save meta assets as JSON")]
	save_meta_assets: bool,

	#[arg(long, help = "validate the data that's being processed")]
	validate: bool,

	#[arg(short = 'n', long, help = "do not write any files")]
	dry: bool,

	#[arg(short = 'q', long, help = "output only errors and warnings")]
	quiet: bool,

	#[arg(short = 'v', long, help = "output verbose information")]
	verbose: bool,

	#[arg(long, help = "Only process assets that match these strings")]
	filter: Vec<String>,
}

fn main() {
	akizuki::format::oodle::init();

	let mut args = Cli::parse();

	let mut log_level = LevelFilter::Info;
	if args.quiet {
		log_level = LevelFilter::Warn;
	} else if args.verbose {
		log_level = LevelFilter::Debug;
	}

	log::set_max_level(log_level);
	init_logging(log_level);

	args.filter.dedup();

	let mut manager = ResourceManager::new(&args.install_path, args.install_version, args.validate)
		.expect("could not create manager");

	let output_path = Path::new(&args.output_path);

	if args.save_meta_assets {
		if let Err(err) = process_db(&args, output_path, manager.load_asset_database(args.validate)) {
			error!(target: "akizuki::unpack", "unable to export data: {:?}", err);
		}
	}

	if args.save_index && !args.dry {
		if let Err(err) = process_index(output_path) {
			error!(target: "akizuki::unpack", "unable to saave index: {:?}", err);
		}
	}

	for package in manager.packages.values() {
		for asset_id in package.files.keys() {
			if let Err(err) = process_asset(&args, output_path, package, *asset_id) {
				error!(target: "akizuki::unpack", "unable to export asset {:?}: {}", asset_id, err);
			}
		}
	}
}

fn process_index(output_path: &Path) -> Result<()> {
	let asset_dir = &output_path.join("idx/");
	fs::create_dir_all(asset_dir)?;

	save_index(asset_dir.join("resource.json"), ResourceId::clone_map())?;
	save_index(asset_dir.join("string.json"), StringId::clone_map())?;

	Ok(())
}

fn save_index<T: serde::ser::Serialize>(asset_path: PathBuf, map: Option<HashMap<T, String>>) -> Result<()> {
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

fn process_asset(args: &Cli, output_path: &Path, package: &PackageFileSystem, asset_id: ResourceId) -> Result<()> {
	let asset_name = asset_id
		.text()
		.unwrap_or_else(|| format!("unknown/{:016x}", asset_id.value()));

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

fn process_db(args: &Cli, output_path: &Path, db: AkizukiResult<&BigWorldDatabase>) -> Result<()> {
	let db = db?;

	for record_id in db.prototype_lookup.keys() {
		if let Err(err) = process_db_asset(args, output_path, db, *record_id) {
			error!(target: "akizuki::unpack", "unable to export data {:?}: {}", record_id, err);
		}
	}

	Ok(())
}

fn process_db_asset(args: &Cli, output_path: &Path, db: &BigWorldDatabase, record_id: ResourceId) -> Result<()> {
	let record = db.open(record_id)?;
	let asset_name = record_id
		.text()
		.unwrap_or_else(|| format!("unknown/{:016x}", record_id.value()));

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

pub struct PrefixModule;
impl CologStyle for PrefixModule {
	fn format(&self, buf: &mut Formatter, record: &Record<'_>) -> Result<(), std::io::Error> {
		let sep = self.line_separator();
		let prefix = self.prefix_token(&record.level());

		write!(buf, "{}", prefix)?;
		write!(
			buf,
			"{}{}{}",
			"[".blue().bold(),
			format!("{}", buf.timestamp()).bright_cyan(),
			"]".blue().bold()
		)?;
		write!(
			buf,
			"{}{}{}",
			"[".blue().bold(),
			record.target().bright_purple(),
			"] ".blue().bold()
		)?;
		writeln!(buf, "{}", record.args().to_string().replace('\n', &sep))?;

		Ok(())
	}
}
pub fn init_logging(filter: LevelFilter) {
	let mut builder = colog::basic_builder();
	builder.format(colog::formatter(PrefixModule));
	builder.filter(None, filter);
	builder.init();
}
