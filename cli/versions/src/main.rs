// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use akizuki::pfs::PackageFileSystem;

use log::LevelFilter;
use pelite::PeFile;
use walkdir::WalkDir;

use std::collections::{HashMap, HashSet};
use std::env;
use std::error::Error;
use std::fs::File;
use std::io::Read;
use std::path::Path;

fn main() {
	akizuki_cli::init_logging(LevelFilter::Debug);

	let paths: Vec<String> = env::args().skip(1).collect();

	match parse_game_pe(&paths) {
		Ok(versions) => {
			parse_versions(paths, versions);
		}
		Err(err) => {
			log::error!("{}", err);
			return;
		}
	}
}

fn parse_versions(paths: Vec<String>, versions: HashMap<u64, String>) {
	let mut parsed_versions: HashSet<u64> = HashSet::new();

	for dir in paths {
		for path in WalkDir::new(dir).into_iter().filter_map(|x| x.ok()).filter(|x| x.file_type().is_file()) {
			if path.path().extension() != Some("idx".as_ref()) {
				continue;
			}

			parse_version(path.path(), &mut parsed_versions, &versions);
		}
	}
}

fn parse_version(path: &Path, parsed_versions: &mut HashSet<u64>, versions: &HashMap<u64, String>) -> Option<()> {
	let version = path.parent()?.parent()?.file_name()?.to_str()?.parse::<u64>().ok()?;

	if parsed_versions.contains(&version) {
		return None;
	}

	let version_str = versions.get(&version)?;

	let idx_path = path.parent()?;
	let pkgs_path_buf = idx_path.join("../../../res_packages").canonicalize().ok()?;
	let pkgs_path = pkgs_path_buf.as_path();

	let pkg = match PackageFileSystem::new(pkgs_path, &path.to_path_buf(), false) {
		Ok(pkg) => pkg,
		Err(err) => {
			log::error!("failed parsing {}.{}, got {}", version_str, version, err);
			return None;
		}
	};

	log::info!("parsing version {}.{}...", version_str, version);

	Some(())
}

fn parse_game_pe(paths: &Vec<String>) -> Result<HashMap<u64, String>, Box<dyn Error>> {
	let mut versions: HashMap<u64, String> = HashMap::new();

	for dir in paths {
		for path in WalkDir::new(dir).into_iter().filter_map(|x| x.ok()).filter(|x| x.file_type().is_file()) {
			if path.path().extension() != Some("exe".as_ref()) {
				continue;
			}

			let mut pe_file = File::open(path.path())?;
			let mut pe_bytes = Vec::<u8>::new();
			pe_file.read_to_end(&mut pe_bytes)?;
			let pe = PeFile::from_bytes(&pe_bytes)?;
			let version_info = pe.resources()?.version_info()?;
			for (_, strings) in version_info.file_info().strings {
				if let Some(product_version) = strings.get("ProductVersion") {
					let parts = product_version.split(',').filter_map(|x| x.trim().parse().ok()).collect::<Vec<u64>>();
					if parts.len() != 4 {
						continue;
					}

					if parts[3] == 0 {
						continue;
					}

					if let Some(version) = versions.insert(parts[3], format!("{}.{}.{}", parts[0], parts[1], parts[2])) {
						log::info!("{} = {}", parts[3], version);
					}
				}
			}
		}
	}

	Ok(versions)
}
