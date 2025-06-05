use colog::format::CologStyle;
use colored::Colorize;
use env_logger::fmt::Formatter;
use log::{LevelFilter, Record};
use std::io::Write;

pub struct PrefixModule;

//noinspection DuplicatedCode
impl CologStyle for PrefixModule {
	fn format(&self, buf: &mut Formatter, record: &Record<'_>) -> Result<(), std::io::Error> {
		let sep = self.line_separator();
		let prefix = self.prefix_token(&record.level());

		write!(buf, "{}", prefix)?;
		write!(buf, "{}{}{}", "[".blue().bold(), format!("{}", buf.timestamp()).bright_cyan(), "]".blue().bold())?;
		write!(buf, "{}{}{}", "[".blue().bold(), record.target().bright_purple(), "] ".blue().bold())?;
		writeln!(buf, "{}", record.args().to_string().replace('\n', &sep))?;

		Ok(())
	}
}

//noinspection DuplicatedCode
pub fn init_logging(filter: LevelFilter) {
	log::set_max_level(filter);

	let mut builder = colog::basic_builder();
	builder.format(colog::formatter(PrefixModule));
	builder.filter(None, filter);
	builder.init();
}
