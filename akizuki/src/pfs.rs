// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::format::bigworld::{BigWorldFileHeader, BigWorldMagic};
use crate::format::pfs::PackageCompressionType::DeflateCompression;
use crate::format::pfs::{PackageFile, PackageFileHeader, PackageFileName, PackageName};
use crate::identifiers::ResourceId;

use binrw::io::BufReader;
use binrw::{BinRead, BinResult, NullString, VecArgs};
use colored::Colorize;
use crc::Crc;
use flate2::FlushDecompress;
use log::{debug, error, info};
use memmap2::Mmap;

use std::collections::HashMap;
use std::fs::File;
use std::io::{Error, ErrorKind, Seek, SeekFrom::Start};
use std::path::{Path, PathBuf};

pub struct PackageFileSystem {
	pub name: String,
	pub files: HashMap<ResourceId, PackageFile>,
	pub streams: HashMap<ResourceId, Mmap>,
}

impl PackageFileSystem {
	pub fn new(pkg_directory: &Path, idx_path: &PathBuf, validate: bool) -> BinResult<PackageFileSystem> {
		let name = Path::file_stem(idx_path).unwrap_or_default().to_os_string().into_string().unwrap_or_default();
		info!("loading {}", name.green());

		let mut reader = BufReader::new(File::open(idx_path)?);
		let bw_header = BigWorldFileHeader::read_ne(&mut reader)?;

		bw_header.validate(BigWorldMagic::PFSIndex, 2, validate, &mut reader)?;

		let header = PackageFileHeader::read_ne(&mut reader)?;

		let names = read_names(&mut reader, &header)?;
		let files = read_files(&mut reader, &header)?;
		let streams = read_streams(&mut reader, &header, pkg_directory)?;

		for file_id in files.keys() {
			let mut target_id = file_id;

			let mut path = Option::<PathBuf>::None;
			while target_id.is_valid() {
				let Some((file_name, parent_id)) = names.get(target_id) else {
					break;
				};

				match path {
					Some(some_path) => path = Some(PathBuf::from(file_name).join(some_path)),
					None => path = Some(PathBuf::from(file_name)),
				}

				target_id = parent_id;
			}

			if let Some(path) = path {
				ResourceId::insert(file_id, path.to_str().unwrap_or_default());
			}
		}

		Ok(PackageFileSystem { name, files, streams })
	}

	const CRC: Crc<u32> = Crc::<u32>::new(&crc::CRC_32_ISO_HDLC);

	pub fn open(&self, id: &ResourceId, validate: bool) -> Option<Vec<u8>> {
		let info = self.files.get(id)?;
		let stream = self.streams.get(&info.package_id)?;

		let data = match read_data_from_stream(stream, info) {
			Ok(data) => data,
			Err(err) => {
				error!("{:?} errored {:?}", id, err);
				return None;
			}
		};

		if validate {
			let hash = PackageFileSystem::CRC.checksum(data.as_slice());
			if hash != info.hash {
				error!("{:?} has an invalid checksum!", id);
				return None;
			}

			debug!("{:?} passed validation", id);
		}

		Some(data)
	}
}

fn read_data_from_stream(stream: &Mmap, info: &PackageFile) -> Result<Vec<u8>, Error> {
	if info.flags > 1 {
		return Err(Error::new(ErrorKind::InvalidData, "invalid file flags"));
	}

	if (info.flags & 1) == 1 {
		if info.compression_type != DeflateCompression {
			return Err(Error::new(ErrorKind::InvalidData, "invalid file compression"));
		}

		let mut data = vec![0; info.uncompressed_size as usize];
		let compressed_data = &stream[info.offset as usize..(info.offset + info.compressed_size as u64) as usize];
		let mut flate = flate2::Decompress::new(false);
		flate.decompress(compressed_data, data.as_mut_slice(), FlushDecompress::Finish)?;
		Ok(data)
	} else {
		Ok(stream[info.offset as usize..(info.offset + info.uncompressed_size) as usize].to_vec())
	}
}

fn read_names(mut reader: &mut BufReader<File>, header: &PackageFileHeader) -> BinResult<HashMap<ResourceId, (String, ResourceId)>> {
	reader.seek(Start(header.relative_position.pos + header.name_offset))?;

	let names = Vec::<PackageFileName>::read_ne_args(&mut reader, VecArgs { count: header.name_count as usize, inner: <_>::default() })?;
	let mut name_map = HashMap::<ResourceId, (String, ResourceId)>::new();
	for name in names {
		reader.seek(Start(name.name.relative_position.pos + name.name.offset))?;
		name_map.insert(name.name.id, (NullString::read_ne(&mut reader)?.to_string(), name.parent_id));
	}

	Ok(name_map)
}

fn read_files(mut reader: &mut BufReader<File>, header: &PackageFileHeader) -> BinResult<HashMap<ResourceId, PackageFile>> {
	reader.seek(Start(header.relative_position.pos + header.file_offset))?;

	let files = Vec::<PackageFile>::read_ne_args(&mut reader, VecArgs { count: header.file_count as usize, inner: <_>::default() })?;
	let mut file_map = HashMap::<ResourceId, PackageFile>::new();
	for file in files {
		file_map.insert(file.id, file);
	}

	Ok(file_map)
}

fn read_streams(mut reader: &mut BufReader<File>, header: &PackageFileHeader, pkg_path: &Path) -> BinResult<HashMap<ResourceId, Mmap>> {
	reader.seek(Start(header.relative_position.pos + header.pkgs_offset))?;

	let names = Vec::<PackageName>::read_ne_args(&mut reader, VecArgs { count: header.pkgs_count as usize, inner: <_>::default() })?;
	let mut name_map = HashMap::<ResourceId, Mmap>::new();
	for name in names {
		reader.seek(Start(name.relative_position.pos + name.offset))?;
		let string = NullString::read_ne(&mut reader)?.to_string();
		ResourceId::insert(&name.id, &string);
		let mut target_pkg_path = pkg_path.to_path_buf();
		target_pkg_path.push(string);
		name_map.insert(name.id, unsafe { Mmap::map(&File::open(target_pkg_path)?)? });
	}

	Ok(name_map)
}
