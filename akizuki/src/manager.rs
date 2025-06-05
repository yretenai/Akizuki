// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::bigworld::BigWorldDatabase;
use crate::error::{AkizukiError, AkizukiResult};
use crate::identifiers::ResourceId;
use crate::pfs::PackageFileSystem;

use std::collections::HashMap;
use std::fs;
use std::path::{Path, PathBuf};
use colored::Colorize;
use log::info;

pub struct ResourceManager {
	pub packages: HashMap<ResourceId, PackageFileSystem>,
	pub lookup: HashMap<ResourceId, ResourceId>,
	pub big_world_database: Option<BigWorldDatabase>,
	pub install_path: String,
	pub install_version: i64,
}

impl ResourceManager {
	pub fn new(install_path: &String, install_version: Option<i64>, should_validate: bool) -> Option<Self> {
		let install = Path::new(install_path);
		let pkg_path = install.join("res_packages");

		let mut idx_path = install.join("bin");

		let version = match install_version {
			Some(install_version) => install_version,
			None => find_install_version(&idx_path).ok()?,
		};

		idx_path.push(version.to_string());
		idx_path.push("idx");

		let packages = load_idx(&pkg_path, &idx_path, should_validate).ok()?;

		let mut lookup = HashMap::<ResourceId, ResourceId>::new();
		for (package_id, package) in &packages {
			for file_id in package.files.keys() {
				lookup.insert(*file_id, *package_id);
			}
		}

		Some(ResourceManager {
			packages,
			lookup,
			install_path: install_path.clone(),
			install_version: version,
			big_world_database: None,
		})
	}

	pub fn load_asset(&self, resource_id: ResourceId, validate: bool) -> AkizukiResult<Vec<u8>> {
		let pkg_id = self.lookup.get(&resource_id).ok_or(AkizukiError::AssetNotFound(resource_id))?;
		let pkg = self.packages.get(pkg_id).ok_or(AkizukiError::AssetNotFound(*pkg_id))?;
		pkg.open(resource_id, validate)
	}

	pub fn load_asset_database(&mut self, validate: bool) -> AkizukiResult<&BigWorldDatabase> {
		if self.big_world_database.is_none() {
			let asset_bin = self.load_asset(ResourceId::new("content/assets.bin"), false)?;
			info!("loading asset db");
			self.big_world_database = Some(BigWorldDatabase::new(asset_bin, validate, false)?);
		}

		Ok(self
			.big_world_database
			.as_ref()
			.expect("should not have reached this point without deserializing the database or erroring normally"))
	}
}

fn load_idx(packages_path: &Path, idx_path: &PathBuf, should_validate: bool) -> AkizukiResult<HashMap<ResourceId, PackageFileSystem>> {
	if !idx_path.is_dir() {
		return Err(AkizukiError::InvalidInstall);
	}

	let entries: Vec<_> = fs::read_dir(idx_path)?
		.filter_map(Result::ok)
		.filter(|entry| {
			let path = entry.path();
			if path.is_dir() {
				return false;
			}

			let Some(ext) = path.extension() else { return false };

			ext.eq_ignore_ascii_case("idx")
		})
		.collect();

	Ok(entries
		.into_iter()
		.filter_map(|entry| {
			info!("loading {}", entry.file_name().to_str().unwrap_or("package").green());
			PackageFileSystem::new(packages_path, &entry.path(), should_validate).ok()
		})
		.map(|pkg| (ResourceId::new(&pkg.name), pkg))
		.collect())
}

fn find_install_version(bin_path: &PathBuf) -> AkizukiResult<i64> {
	let mut max_number: i64 = 0;

	for entry in fs::read_dir(bin_path)? {
		let entry = entry?;
		let path = entry.path();

		if !path.is_dir() {
			continue;
		}

		let Some(folder_num) = path.file_name().and_then(|n| n.to_str()).and_then(|s| s.parse::<i64>().ok()) else {
			continue;
		};

		if folder_num > max_number {
			max_number = folder_num;
		}
	}

	if max_number == 0 { Err(AkizukiError::InvalidInstall) } else { Ok(max_number) }
}
