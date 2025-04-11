// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use binrw::{BinRead, BinReaderExt, BinResult, Endian};
use std::io::{Read, Seek};

// wrappers for BinRead...

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Vec2(glam::Vec2);

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Vec3(glam::Vec3);

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Vec4(glam::Vec4);

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Mat4(glam::Mat4);

#[derive(BinRead, Debug, Clone, Copy, Ord, PartialOrd, Eq, PartialEq)]
#[br(repr = u8)]
pub enum FlagBool {
	False,
	True,
}

#[derive(BinRead, Debug, Clone, Copy, PartialEq)]
#[br()]
#[cfg_attr(feature = "serialize", derive(serde::Serialize))]
pub struct BoundingBox {
	#[br(pad_after = 4)]
	pub min: Vec3,
	#[br(pad_after = 4)]
	pub max: Vec3,
}

macro_rules! passthrough_serialize {
	($name:ident) => {
		impl serde::Serialize for $name {
			fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
			where
				S: serde::Serializer,
			{
				self.0.serialize(serializer)
			}
		}
	};
}

macro_rules! passthrough_read {
	($name:ident, $inner:ident, $count:literal) => {
		impl BinRead for $name {
			type Args<'a> = ();

			fn read_options<R: Read + Seek>(reader: &mut R, endian: Endian, _args: Self::Args<'_>) -> BinResult<Self> {
				Ok($name(glam::$name::from_array(
					reader.read_type::<[$inner; $count]>(endian)?,
				)))
			}
		}

		impl From<$name> for glam::$name {
			fn from(value: $name) -> glam::$name {
				value.0
			}
		}
	};
}

macro_rules! passthrough_ref_read {
	($name:ident, $func:ident, $inner:ident, $count:literal) => {
		impl BinRead for $name {
			type Args<'a> = ();

			fn read_options<R: Read + Seek>(reader: &mut R, endian: Endian, _args: Self::Args<'_>) -> BinResult<Self> {
				Ok($name(glam::$name::$func(
					&reader.read_type::<[$inner; $count]>(endian)?,
				)))
			}
		}

		impl From<$name> for glam::$name {
			fn from(value: $name) -> glam::$name {
				value.0
			}
		}
	};
}

#[cfg(feature = "serialize")]
passthrough_serialize!(Vec2);
passthrough_read!(Vec2, f32, 2);

#[cfg(feature = "serialize")]
passthrough_serialize!(Vec3);
passthrough_read!(Vec3, f32, 3);

#[cfg(feature = "serialize")]
passthrough_serialize!(Vec4);
passthrough_read!(Vec4, f32, 4);

#[cfg(feature = "serialize")]
passthrough_serialize!(Mat4);
passthrough_ref_read!(Mat4, from_cols_array, f32, 16);

impl From<FlagBool> for bool {
	fn from(value: FlagBool) -> Self {
		match value {
			FlagBool::False => false,
			FlagBool::True => true,
		}
	}
}
