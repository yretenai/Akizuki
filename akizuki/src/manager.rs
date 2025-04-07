// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::identifiers::ResourceId;
use crate::pfs::PackageFileSystem;

use colored::Colorize;
use log::error;

use std::collections::HashMap;
use std::io::ErrorKind;
use std::path::{Path, PathBuf};
use std::{fs, io};

pub struct ResourceManager {
	pub packages: HashMap<ResourceId, PackageFileSystem>,
	pub lookup: HashMap<ResourceId, ResourceId>,
}

impl ResourceManager {
	pub fn new(install_path: &String, install_version: Option<i64>, should_validate: bool) -> Option<Self> {
		let install = Path::new(install_path);
		let pkg_path = install.join("res_packages");

		let mut idx_path = install.join("bin");

		idx_path.push(match install_version {
			Some(install_version) => install_version.to_string(),
			_ => find_install_version(&idx_path).unwrap_or_else(|err| {
				panic!("could not determine game version!\n{:?}", err);
			}),
		});

		idx_path.push("idx");

		let packages = match load_idx(&pkg_path, &idx_path, should_validate) {
			Ok(packages) => packages,
			Err(err) => {
				error!("could not load packages: {:?}", err);
				return None;
			}
		};

		let mut lookup = HashMap::<ResourceId, ResourceId>::new();
		for (package_id, package) in &packages {
			for file_id in package.files.keys() {
				lookup.insert(*file_id, *package_id);
			}
		}

		Some(ResourceManager { packages, lookup })
	}
}

fn load_idx(packages_path: &Path, idx_path: &PathBuf, should_validate: bool) -> io::Result<HashMap<ResourceId, PackageFileSystem>> {
	let mut packages = HashMap::<ResourceId, PackageFileSystem>::new();

	if !idx_path.is_dir() {
		return Ok(packages);
	}

	for entry in fs::read_dir(idx_path)? {
		let entry = entry?;
		let path = entry.path();
		if path.is_dir() || path.extension().unwrap_or_default().to_str().unwrap_or_default() == ".idx" {
			continue;
		}

		match PackageFileSystem::new(packages_path, &path, should_validate) {
			Ok(pkg) => {
				packages.insert(ResourceId::new(&pkg.name), pkg);
			}
			Err(err) => {
				error!("{}", err.to_string().red());
			}
		}
	}

	Ok(packages)
}

fn find_install_version(bin_path: &PathBuf) -> Result<String, io::Error> {
	let mut max_number: i64 = 0;
	let mut max_folder: Option<PathBuf> = Option::default();

	for entry in fs::read_dir(bin_path)? {
		let entry = entry?;
		let path = entry.path();

		if !path.is_dir() {
			continue;
		}

		let folder_name = path.file_name().and_then(|n| n.to_str()).unwrap_or_default();
		let folder_num = folder_name.parse::<i64>().unwrap_or_default();
		if folder_num > max_number {
			max_number = folder_num;
			max_folder = Some(PathBuf::from(folder_name));
		}
	}

	match max_folder {
		Some(folder) => Ok(folder.file_stem().unwrap_or_default().to_os_string().into_string().unwrap_or_default()),
		_ => Err(io::Error::new(ErrorKind::NotFound, "no versions present")),
	}
}
