// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use akizuki::bigworld::BigWorldDatabase;
use akizuki::identifiers::ResourceId;
use akizuki::pfs::PackageFileSystem;
use akizuki_macro::akizuki_resource;

use log::LevelFilter;
use pelite::PeFile;
use semver::Version;
use walkdir::WalkDir;

use std::collections::{HashMap, HashSet};
use std::env;
use std::error::Error;
use std::fs::File;
use std::io::{BufWriter, Read, Write};
use std::path::Path;

const NEWLINE: [u8; 1] = [0xA];

#[derive(Debug, serde::Serialize)]
struct VersionInfo {
	pub version: Version,
	pub version_id: u32,
}

fn main() {
	akizuki_cli::init_logging(LevelFilter::Debug);

	let paths: Vec<String> = env::args().skip(1).collect();

	match parse_game_pe(&paths) {
		Ok(versions) => {
			if let Err(err) = parse_versions(paths, versions) {
				log::error!("{}", err);
			}
		}
		Err(err) => {
			log::error!("{}", err);
			return;
		}
	}
}

const ASSETS_BIN: ResourceId = akizuki_resource!("content/assets.bin");

fn parse_versions(paths: Vec<String>, versions: HashMap<u64, Version>) -> Result<(), Box<dyn Error>> {
	let mut parsed_versions: HashSet<u64> = HashSet::new();

	let mut earliest_versions: HashMap<u32, HashMap<u32, VersionInfo>> = HashMap::new();

	for dir in paths {
		for path in WalkDir::new(dir).into_iter().filter_map(|x| x.ok()).filter(|x| x.file_type().is_file()) {
			if path.path().extension() != Some("idx".as_ref()) {
				continue;
			}

			parse_version(path.path(), &mut parsed_versions, &mut earliest_versions, &versions);
		}
	}

	let file = File::create("versions.json")?;
	file.set_len(0)?;
	let mut writer = BufWriter::new(file);
	serde_json::to_writer_pretty(&mut writer, &earliest_versions)?;
	writer.write_all(&NEWLINE)?;

	Ok(())
}

fn parse_version(
	path: &Path,
	parsed_versions: &mut HashSet<u64>,
	earliest_versions: &mut HashMap<u32, HashMap<u32, VersionInfo>>,
	versions: &HashMap<u64, Version>,
) -> Option<()> {
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

	let asset = match pkg.open(ASSETS_BIN, false) {
		Ok(asset) => asset,
		Err(err) => {
			log::error!("failed opening assets.bin for {}.{}, got {}", version_str, version, err);
			return None;
		}
	};

	let bwdb = match BigWorldDatabase::new(asset, false) {
		Ok(asset) => asset,
		Err(err) => {
			log::error!("failed parsing assets.bin for {}.{}, got {}", version_str, version, err);
			return None;
		}
	};

	for record in bwdb.table_headers {
		let table_versions = earliest_versions.entry(record.id.0).or_insert_with(HashMap::new);

		let info = VersionInfo {
			version: version_str.clone(),
			version_id: record.version,
		};

		if let Some(table_version) = table_versions.get(&record.version) {
			if table_version.version < info.version {
				table_versions.insert(record.version, info);
			}
		} else {
			table_versions.insert(record.version, info);
		}
	}

	Some(())
}

fn parse_game_pe(paths: &Vec<String>) -> Result<HashMap<u64, Version>, Box<dyn Error>> {
	let mut versions: HashMap<u64, Version> = HashMap::new();

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

					if let Some(version) = versions.insert(parts[3], Version::new(parts[0], parts[1], parts[2])) {
						log::info!("{} = {}", parts[3], version);
					}
				}
			}
		}
	}

	Ok(versions)
}
