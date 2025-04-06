// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::pfs::PackageFileSystem;
use colored::Colorize;
use log::error;
use std::io::ErrorKind;
use std::path::PathBuf;
use std::{fs, io};

#[derive(Debug)]
pub struct ResourceManager {
	pub packages: Vec<PackageFileSystem>,
}

impl ResourceManager {
	pub fn new(install_path: &String, install_version: Option<i64>, should_validate: bool) -> Self {
		let install = PathBuf::from(install_path);
		let mut pkg_path = install.clone();
		pkg_path.push("res_packages");

		let mut idx_path = install.clone();
		idx_path.push("bin");

		idx_path.push(match install_version {
			Some(install_version) => install_version.to_string(),
			_ => find_install_version(&idx_path).unwrap_or_else(|err| {
				panic!("could not determine game version!\n{:?}", err);
			}),
		});

		idx_path.push("idx");

		let packages = load_idx(&pkg_path, &idx_path, should_validate).unwrap_or_default();

		return ResourceManager { packages };
	}
}

fn load_idx(packages_path: &PathBuf, idx_path: &PathBuf, should_validate: bool) -> io::Result<Vec<PackageFileSystem>> {
	let mut packages = Vec::<PackageFileSystem>::new();

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
			Ok(pkg) => packages.push(pkg),
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

	return match max_folder {
		Some(folder) => Ok(folder.file_stem().unwrap_or_default().to_os_string().into_string().unwrap_or_default()),
		_ => Err(io::Error::new(ErrorKind::NotFound, "no versions present")),
	};
}
