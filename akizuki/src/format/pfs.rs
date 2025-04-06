// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use crate::identifiers::ResourceId;

use binrw::{BinRead, PosValue};

#[derive(BinRead, Debug, Clone, Ord, PartialOrd, Eq, PartialEq)]
#[br(repr = u32)]
pub enum PackageCompressionType {
	NoCompression = 0,
	DeflateCompression = 5,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct PackageFile {
	pub id: ResourceId,
	pub package_id: ResourceId,
	pub offset: u64,
	pub compression_type: PackageCompressionType,
	pub flags: u32,
	pub compressed_size: u32,
	pub hash: u32,
	pub uncompressed_size: u64,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct PackageFileHeader {
	pub relative_position: PosValue<()>,

	pub name_count: u32,
	pub file_count: u32,
	#[br(pad_after = 4)]
	pub pkgs_count: u32,

	pub name_offset: u64,
	pub file_offset: u64,
	pub pkgs_offset: u64,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct PackageFileName {
	pub name: PackageName,
	pub parent_id: ResourceId,
}

#[derive(BinRead, Debug, Clone)]
#[br()]
pub struct PackageName {
	pub relative_position: PosValue<()>,
	pub length: u64,
	pub offset: u64,
	pub id: ResourceId,
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
