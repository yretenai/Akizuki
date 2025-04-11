// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use binrw::BinRead;
use colored::Colorize;
use once_cell::sync::Lazy;

use akizuki_macro::akizuki_id;
use std::collections::HashMap;
use std::fmt;
use std::sync::RwLock;

#[repr(C)]
#[derive(BinRead, Clone, Copy, PartialEq, Eq, Hash)]
#[br(repr = u32)]
pub struct StringId(pub u32);

static STRING_LOOKUP: Lazy<RwLock<HashMap<u32, String>>> = Lazy::new(|| {
	let mut result = HashMap::<u32, String>::new();

	result.insert(akizuki_id!("MaterialPrototype").0, "MaterialPrototype".to_string());
	result.insert(akizuki_id!("VisualPrototype").0, "VisualPrototype".to_string());
	result.insert(akizuki_id!("ModelPrototype").0, "ModelPrototype".to_string());
	result.insert(akizuki_id!("SkeletonPrototype").0, "SkeletonPrototype".to_string());
	result.insert(akizuki_id!("PointLightPrototype").0, "PointLightPrototype".to_string());
	result.insert(akizuki_id!("AtlasContourProto").0, "AtlasContourProto".to_string());
	result.insert(akizuki_id!("EffectPrototype").0, "EffectPrototype".to_string());
	result.insert(akizuki_id!("TrailPrototype").0, "TrailPrototype".to_string());
	result.insert(akizuki_id!("MiscTypePrototype").0, "MiscTypePrototype".to_string());
	result.insert(
		akizuki_id!("MiscSettingsPrototype").0,
		"MiscSettingsPrototype".to_string(),
	);
	result.insert(
		akizuki_id!("VelocityFieldPrototype").0,
		"VelocityFieldPrototype".to_string(),
	);
	result.insert(
		akizuki_id!("EffectPresetPrototype").0,
		"EffectPresetPrototype".to_string(),
	);
	result.insert(
		akizuki_id!("EffectMetadataPrototype").0,
		"EffectMetadataPrototype".to_string(),
	);

	RwLock::new(result)
});

#[repr(C)]
#[derive(BinRead, Clone, Copy, PartialEq, Eq, Hash)]
#[br(repr = u64)]
pub struct ResourceId(pub u64);

static RESOURCE_LOOKUP: Lazy<RwLock<HashMap<u64, String>>> = Lazy::new(|| RwLock::new(HashMap::new()));

impl StringId {
	pub fn new(s: &str) -> Self {
		// there are zero good murmurhash3_32 crates.
		Self(akizuki_common::mmh3::mmh3_32(s.as_ref()))
	}

	#[inline]
	pub fn value(&self) -> u32 {
		if self.is_valid() { self.0 } else { 0xFFFFFFFF }
	}

	//noinspection DuplicatedCode
	#[inline]
	pub fn text(&self) -> Option<String> {
		if self.is_valid() {
			STRING_LOOKUP.read().unwrap().get(&self.0).cloned()
		} else {
			None
		}
	}

	#[inline]
	pub fn insert(id: StringId, s: &str) {
		if !id.is_valid() {
			return;
		}

		STRING_LOOKUP.write().unwrap().insert(id.0, s.to_owned());
	}

	pub fn is_valid(&self) -> bool {
		self.0 > 0 && self.0 < 0xFFFFFFFF
	}
}

impl ResourceId {
	pub fn new(s: &str) -> Self {
		Self(cityhasher::hash(s))
	}

	#[inline]
	pub fn value(&self) -> u64 {
		if self.is_valid() { self.0 } else { 0xFFFFFFFFFFFFFFFF }
	}

	//noinspection DuplicatedCode
	#[inline]
	pub fn text(&self) -> Option<String> {
		if self.is_valid() {
			RESOURCE_LOOKUP.read().unwrap().get(&self.0).cloned()
		} else {
			None
		}
	}

	#[inline]
	pub fn insert(id: ResourceId, s: &str) {
		if !id.is_valid() {
			return;
		}

		RESOURCE_LOOKUP.write().unwrap().insert(id.0, s.to_owned());
	}

	pub fn is_valid(&self) -> bool {
		self.0 > 0 && self.0 < 0xFFFFFFFFFFFFFFFF
	}
}

impl PartialEq<u32> for StringId {
	fn eq(&self, other: &u32) -> bool {
		self.0.eq(other)
	}
}

impl PartialEq<u64> for ResourceId {
	fn eq(&self, other: &u64) -> bool {
		self.0.eq(other)
	}
}

impl fmt::Debug for StringId {
	fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
		match self.text() {
			Some(s) => write!(f, "\"{}\" ({})", s.blue(), format!("0x{:08x}", self.0).yellow()),
			None => write!(f, "{} ({})", "<unknown>".red(), format!("0x{:08x}", self.0).yellow()),
		}
	}
}

impl fmt::Debug for ResourceId {
	fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
		match self.text() {
			Some(s) => write!(f, "\"{}\" ({})", s.blue(), format!("0x{:016x}", self.0).yellow()),
			None => write!(f, "{} ({})", "<unknown>".red(), format!("0x{:016x}", self.0).yellow()),
		}
	}
}

impl From<u32> for StringId {
	fn from(value: u32) -> Self {
		Self(value)
	}
}

impl From<u64> for ResourceId {
	fn from(value: u64) -> Self {
		Self(value)
	}
}

#[cfg(feature = "serialize")]
impl serde::Serialize for StringId {
	fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
	where
		S: serde::Serializer,
	{
		if serializer.is_human_readable() {
			match self.text() {
				Some(text) => serializer.serialize_str(&text),
				None => serializer.serialize_u32(self.0),
			}
		} else {
			serializer.serialize_u32(self.0)
		}
	}
}

#[cfg(feature = "serialize")]
impl serde::Serialize for ResourceId {
	fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
	where
		S: serde::Serializer,
	{
		if serializer.is_human_readable() {
			match self.text() {
				Some(text) => serializer.serialize_str(&text),
				None => serializer.serialize_u64(self.0),
			}
		} else {
			serializer.serialize_u64(self.0)
		}
	}
}

#[cfg(test)]
mod tests {
	use super::*;

	use colored::control::SHOULD_COLORIZE;

	#[test]
	fn test_string_id() {
		const TEST_STR: &str = "Akizuki";

		assert_eq!(StringId::new(TEST_STR).value(), 0x8d949450);
	}

	#[test]
	fn test_string_id4() {
		const TEST_STR: &str = "Akizuki_";

		assert_eq!(StringId::new(TEST_STR).value(), 0xe344aed1);
	}

	#[test]
	fn test_string_text() {
		const TEST_STR: &str = "Akizuki";
		StringId::insert(StringId(0x8d949450), TEST_STR);

		assert_eq!(StringId::new(TEST_STR).text().unwrap(), TEST_STR);
	}

	#[test]
	fn test_string_debug() {
		const TEST_STR: &str = "Akizuki";
		StringId::insert(StringId(0x8d949450), TEST_STR);

		SHOULD_COLORIZE.set_override(false);
		assert_eq!(format!("{:?}", StringId(0x8d949450)), "\"Akizuki\" (0x8d949450)");
	}

	#[test]
	fn test_resource_id() {
		const TEST_STR: &str = "content/gameplay/japan/ship/destroyer/JSD011_Akizuki_1944/JSD011_Akizuki_1944.model";
		assert_eq!(ResourceId::new(TEST_STR).value(), 0x0df5a921212a899e);
	}

	#[test]
	fn test_resource_id8() {
		const TEST_STR: &str = "content/gameplay/japan/ship/destroyer/JSD011_Akizuki_1944/JSD011_Akizuki_1944.mo";
		assert_eq!(ResourceId::new(TEST_STR).value(), 0xa8fa81214165ecb);
	}

	#[test]
	fn test_resource_text() {
		const TEST_STR: &str = "content/gameplay/japan/ship/destroyer/JSD011_Akizuki_1944/JSD011_Akizuki_1944.model";
		ResourceId::insert(ResourceId(0x0df5a921212a899e), TEST_STR);

		assert_eq!(ResourceId(0x0df5a921212a899e).text().unwrap(), TEST_STR);
	}

	#[test]
	fn test_resource_debug() {
		const TEST_STR: &str = "content/gameplay/japan/ship/destroyer/JSD011_Akizuki_1944/JSD011_Akizuki_1944.model";
		ResourceId::insert(ResourceId(0x0df5a921212a899e), TEST_STR);

		SHOULD_COLORIZE.set_override(false);
		assert_eq!(
			format!("{:?}", ResourceId(0x0df5a921212a899e)),
			"\"content/gameplay/japan/ship/destroyer/JSD011_Akizuki_1944/JSD011_Akizuki_1944.model\" (0x0df5a921212a899e)"
		);
	}
}
