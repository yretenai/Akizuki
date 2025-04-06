// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::format::bigworld::{BigWorldFileHeader, BigWorldMagic};
use crate::format::pfs::{PackageFile, PackageFileHeader};
use crate::identifiers::ResourceId;

use binrw::io::BufReader;
use binrw::{BinRead, BinResult};

use colored::Colorize;
use log::info;
use std::collections::HashMap;
use std::fs::File;
use std::path::{Path, PathBuf};

#[derive(Debug, Clone)]
pub struct PackageFileSystem {
	should_validate: bool,
	pub name: String,
	pub files: HashMap<ResourceId, PackageFile>,
	pub streams: HashMap<ResourceId, String>,
}

impl PackageFileSystem {
	pub fn new(pkg_directory: &PathBuf, idx_path: &PathBuf, should_validate: bool) -> BinResult<PackageFileSystem> {
		let name = Path::file_stem(idx_path).unwrap_or_default().to_os_string().into_string().unwrap_or_default();
		info!("loading {}", name.green());

		let mut reader = BufReader::new(File::open(idx_path)?);
		let bw_header = BigWorldFileHeader::read_ne(&mut reader)?;

		bw_header.validate(BigWorldMagic::PFSIndex, 2, should_validate, &mut reader)?;

		let header = PackageFileHeader::read_ne(&mut reader)?;

		return Ok(PackageFileSystem { should_validate, name, files: Default::default(), streams: Default::default() });
	}
}
