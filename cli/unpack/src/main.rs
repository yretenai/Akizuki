// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use akizuki::manager::ResourceManager;
use clap::Parser;
use colog::format::CologStyle;
use colored::Colorize;
use env_logger::fmt::Formatter;
use log::{LevelFilter, Record};
use std::io::{Error, Write};

#[derive(Parser)]
#[command(version, about)]
struct Cli {
	#[arg(index = 1, required = true, help = "path to write files")]
	output_path: String,

	#[arg(index = 2, required = true, help = "path to the game installation directory")]
	install_path: String,

	#[arg(index = 3, help = "version number of the game, if not set will try to find the latest version")]
	install_version: Option<i64>,

	#[arg(long, help = "do not write any files")]
	validate: bool,

	#[arg(short = 'n', help = "do not write any files")]
	dry: bool,

	#[arg(short = 'q', long, help = "output only errors and warnings")]
	quiet: bool,

	#[arg(short = 'v', long, help = "output verbose information")]
	verbose: bool,
}

fn main() {
	let args = Cli::parse();

	let mut log_level = LevelFilter::Info;
	if args.quiet {
		log_level = LevelFilter::Warn;
	} else if args.verbose {
		log_level = LevelFilter::Debug;
	}

	log::set_max_level(log_level);
	init_logging(log_level);

	let manager = ResourceManager::new(&args.install_path, args.install_version, args.validate);
}

pub struct PrefixModule;
impl CologStyle for PrefixModule {
	fn format(&self, buf: &mut Formatter, record: &Record<'_>) -> Result<(), Error> {
		let sep = self.line_separator();
		let prefix = self.prefix_token(&record.level());

		write!(buf, "{}", prefix)?;
		write!(buf, "{}{}{}", "[".blue().bold(), format!("{}", buf.timestamp()).bright_cyan(), "]")?;
		write!(buf, "{}{}{}", "[".blue().bold(), record.target().bright_purple(), "] ".blue().bold())?;
		writeln!(buf, "{}", record.args().to_string().replace('\n', &sep))?;

		return Ok(());
	}
}
pub fn init_logging(filter: LevelFilter) {
	let mut builder = colog::basic_builder();
	builder.format(colog::formatter(PrefixModule));
	builder.filter(None, filter);
	builder.init();
}
