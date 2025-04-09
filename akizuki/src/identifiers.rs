// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use binrw::BinRead;
use colored::Colorize;
use once_cell::sync::Lazy;

use std::collections::HashMap;
use std::fmt;
use std::sync::RwLock;

#[repr(C)]
#[derive(BinRead, Clone, Copy, PartialEq, Eq, Hash)]
#[br(repr = u32)]
pub struct StringId(pub u32);

static STRING_LOOKUP: Lazy<RwLock<HashMap<u32, String>>> = Lazy::new(|| RwLock::new(HashMap::new()));

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
		self.0
	}

	#[inline]
	pub fn text(&self) -> Option<String> {
		STRING_LOOKUP.read().unwrap().get(&self.0).cloned()
	}

	#[inline]
	pub fn insert(id: &StringId, s: &str) {
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
		self.0
	}

	#[inline]
	pub fn text(&self) -> Option<String> {
		RESOURCE_LOOKUP.read().unwrap().get(&self.0).cloned()
	}

	#[inline]
	pub fn insert(id: &ResourceId, s: &str) {
		RESOURCE_LOOKUP.write().unwrap().insert(id.0, s.to_owned());
	}

	pub fn is_valid(&self) -> bool {
		self.0 > 0 && self.0 < 0xFFFFFFFFFFFFFFFF
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
		StringId::insert(&StringId(0x8d949450), TEST_STR);

		assert_eq!(StringId::new(TEST_STR).text().unwrap(), TEST_STR);
	}

	#[test]
	fn test_string_debug() {
		const TEST_STR: &str = "Akizuki";
		StringId::insert(&StringId(0x8d949450), TEST_STR);

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
		ResourceId::insert(&ResourceId(0x0df5a921212a899e), TEST_STR);

		assert_eq!(ResourceId(0x0df5a921212a899e).text().unwrap(), TEST_STR);
	}

	#[test]
	fn test_resource_debug() {
		const TEST_STR: &str = "content/gameplay/japan/ship/destroyer/JSD011_Akizuki_1944/JSD011_Akizuki_1944.model";
		ResourceId::insert(&ResourceId(0x0df5a921212a899e), TEST_STR);

		SHOULD_COLORIZE.set_override(false);
		assert_eq!(format!("{:?}", ResourceId(0x0df5a921212a899e)), "\"content/gameplay/japan/ship/destroyer/JSD011_Akizuki_1944/JSD011_Akizuki_1944.model\" (0x0df5a921212a899e)");
	}
}
