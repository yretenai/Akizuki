// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::identifiers::ResourceId;
use binrw::{BinRead, PosValue};

#[derive(BinRead, Debug, Clone)]
#[br(repr = u32)]
pub enum PackageCompressionType {
	NoCompression,
	DeflateCompression = 5,
}

#[derive(BinRead, Debug, Clone)]
#[br(repr = u32)]
pub enum PackageFileFlags {
	None,
	Compressed = 1,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct PackageFile {
	id: ResourceId,
	package_id: ResourceId,
	offset: i64,
	compression_type: PackageCompressionType,
	flags: PackageFileFlags,
	compressed_size: i32,
	hash: u32,
	uncompressed_size: i64,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct PackageFileHeader {
	relative_position: PosValue<()>,

	name_count: i32,
	file_count: i32,
	#[br(pad_after = 4)]
	pkgs_count: i32,

	name_offset: i64,
	file_offset: i64,
	pkgs_offset: i64,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct PackageFileName {
	name: PackageName,
	parent_id: i64,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct PackageName {
	relative_position: PosValue<()>,
	length: i64,
	offset: i64,
	id: ResourceId,
}

impl PartialEq<ResourceId> for PackageFile {
	fn eq(&self, other: &ResourceId) -> bool {
		self.id.eq(other)
	}
}

impl PartialEq<ResourceId> for PackageName {
	fn eq(&self, other: &ResourceId) -> bool {
		self.id.eq(other)
	}
}

impl PartialEq<ResourceId> for PackageFileName {
	fn eq(&self, other: &ResourceId) -> bool {
		self.name.id.eq(other)
	}
}

impl PartialEq<PackageFileName> for PackageFile {
	fn eq(&self, other: &PackageFileName) -> bool {
		self.id.eq(&other.name.id)
	}
}

impl PartialEq<PackageFile> for PackageFileName {
	fn eq(&self, other: &PackageFile) -> bool {
		self.name.id.eq(&other.id)
	}
}
