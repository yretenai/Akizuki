// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use akizuki::identifiers::ResourceId;
use akizuki::manager::ResourceManager;
use akizuki::pfs::PackageFileSystem;

use clap::Parser;
use colog::format::CologStyle;
use colored::Colorize;
use env_logger::fmt::Formatter;
use log::{LevelFilter, Record, error, info};

use std::fs;
use std::io::{Error, Write};
use std::path::Path;

#[derive(Parser)]
#[command(version, about)]
struct Cli {
	#[arg(index = 1, required = true, help = "path to write files")]
	output_path: String,

	#[arg(index = 2, required = true, help = "path to the game installation directory")]
	install_path: String,

	#[arg(index = 3, help = "version number of the game, if not set will try to find the latest version")]
	install_version: Option<i64>,

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

	let Some(mut manager) = ResourceManager::new(&args.install_path, args.install_version, args.validate) else {
		error!(target: "akizuki::unpack", "could not create manager");
		return;
	};

	match &manager.load_asset_database(true) {
		Ok(_) => {}
		Err(err) => error!("{:?}", err),
	}

	let output_path = Path::new(&args.output_path);

	for package in manager.packages.values() {
		for asset_id in package.files.keys() {
			process_asset(&args, output_path, package, asset_id);
		}
	}
}

fn process_asset(args: &Cli, output_path: &Path, package: &PackageFileSystem, asset_id: &ResourceId) {
	let asset_name = match asset_id.text() {
		Some(asset_name) => asset_name,
		None => format!("unknown/{:016x}", asset_id.value()),
	};

	if !args.filter.is_empty() && !args.filter.iter().any(|v| asset_name.contains(v)) {
		return;
	}

	let Ok(data) = package.open(asset_id, args.validate) else {
		return;
	};

	info!(target: "akizuki::unpack", "Unpacking {:?}", asset_id);

	if args.dry {
		return;
	}

	let asset_path = output_path.join(asset_name);
	let asset_dir = asset_path.parent().unwrap_or(output_path);

	if let Err(err) = fs::create_dir_all(asset_dir) {
		error!(target: "akizuki::unpack", "unable to create path {:?}: {:?}", asset_dir, err.to_string());
		return;
	}

	if let Err(err) = fs::write(&asset_path, data) {
		error!(target: "akizuki::unpack", "unable to write data {:?}: {:?}", asset_dir, err.to_string());
	}
}

pub struct PrefixModule;
impl CologStyle for PrefixModule {
	fn format(&self, buf: &mut Formatter, record: &Record<'_>) -> Result<(), Error> {
		let sep = self.line_separator();
		let prefix = self.prefix_token(&record.level());

		write!(buf, "{}", prefix)?;
		write!(buf, "{}{}{}", "[".blue().bold(), format!("{}", buf.timestamp()).bright_cyan(), "]".blue().bold())?;
		write!(buf, "{}{}{}", "[".blue().bold(), record.target().bright_purple(), "] ".blue().bold())?;
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
