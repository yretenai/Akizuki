// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use once_cell::sync::Lazy;
use std::collections::HashMap;
use std::fmt;
use std::sync::RwLock;

#[repr(C)]
#[derive(Clone, Copy, PartialEq, Eq, Hash)]
pub struct StringId(pub u32);

static STRING_LOOKUP: Lazy<RwLock<HashMap<u32, String>>> =
	Lazy::new(|| RwLock::new(HashMap::new()));

#[repr(C)]
#[derive(Clone, Copy, PartialEq, Eq, Hash)]
pub struct ResourceId(pub u64);

static RESOURCE_LOOKUP: Lazy<RwLock<HashMap<u64, String>>> =
	Lazy::new(|| RwLock::new(HashMap::new()));

#[inline]
fn mmh3_fmix(hash: u32) -> u32 {
	let mut h = hash;
	h ^= h >> 16;
	h = h.wrapping_mul(0x85ebca6b);
	h ^= h >> 13;
	h = h.wrapping_mul(0xc2b2ae35);
	h ^= h >> 16;
	return h;
}

#[inline]
fn mmh3_32(data: impl AsRef<[u8]>) -> u32 {
	let bytes = data.as_ref();

	const C1: u32 = 0xcc9e2d51;
	const C2: u32 = 0x1b873593;
	const E: u32 = 0xe6546b64;

	let mut h1: u32 = 0;
	let mut k1: u32;

	let mut blocks = bytes.chunks_exact(4);
	for block in &mut blocks {
		k1 = bytemuck::pod_read_unaligned::<u32>(block).wrapping_mul(C1);
		k1 = k1.rotate_left(15);
		k1 = k1.wrapping_mul(C2);

		h1 ^= k1;
		h1 = h1.rotate_left(13);
		h1 = h1.wrapping_mul(5).wrapping_add(E);
	}

	k1 = 0;

	let remainder = blocks.remainder();
	let remainder_len = remainder.len();

	// no fallthrough, not going to do a for loop.
	if remainder_len > 2 {
		k1 ^= (remainder[2] as u32) << 16;
	}

	if remainder_len > 1 {
		k1 ^= (remainder[1] as u32) << 8;
	}

	if remainder_len > 0 {
		k1 ^= remainder[0] as u32;
		k1 = k1.wrapping_mul(C1);
		k1 = k1.rotate_left(15);
		k1 = k1.wrapping_mul(C2);
		h1 ^= k1;
	}

	return mmh3_fmix(h1 ^ (bytes.len() as u32));
}

impl StringId {
	pub fn new(s: &str) -> Self {
		// there are zero good murmurhash3_32 crates.
		Self(mmh3_32(s))
	}

	#[inline]
	pub fn value(&self) -> u32 {
		self.0
	}

	#[inline]
	pub fn text(&self) -> Option<String> {
		match STRING_LOOKUP.read().unwrap().get(&self.0) {
			Some(s) => Some(s.clone()),
			None => None,
		}
	}

	#[inline]
	pub fn insert(s: &str, hash: u32) {
		STRING_LOOKUP.write().unwrap().insert(hash, s.to_owned());
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
		match RESOURCE_LOOKUP.read().unwrap().get(&self.0) {
			Some(s) => Some(s.clone()),
			None => None,
		}
	}

	#[inline]
	pub fn insert(s: &str, hash: u64) {
		RESOURCE_LOOKUP.write().unwrap().insert(hash, s.to_owned());
	}
}

impl fmt::Debug for StringId {
	fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
		match self.text() {
			Some(s) => write!(f, "\"{}\" (0x{:08x})", s, self.0),
			None => write!(f, "<unknown> (0x{:08x})", self.0),
		}
	}
}

impl fmt::Debug for ResourceId {
	fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
		match self.text() {
			Some(s) => write!(f, "\"{}\" (0x{:016x})", s, self.0),
			None => write!(f, "<unknown> (0x{:016x})", self.0),
		}
	}
}

#[cfg(test)]
mod tests {
	use super::*;

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
		StringId::insert(TEST_STR, 0x8d949450);

		assert_eq!(StringId::new(TEST_STR).text().unwrap(), TEST_STR);
	}

	#[test]
	fn test_string_debug() {
		const TEST_STR: &str = "Akizuki";
		StringId::insert(TEST_STR, 0x8d949450);

		assert_eq!(
			format!("{:?}", StringId(0x8d949450)),
			"\"Akizuki\" (0x8d949450)"
		);
	}

	#[test]
	fn test_resource_id() {
		const TEST_STR: &str =
			"content/gameplay/japan/ship/destroyer/JSD011_Akizuki_1944/JSD011_Akizuki_1944.model";
		assert_eq!(ResourceId::new(TEST_STR).value(), 0x0df5a921212a899e);
	}

	#[test]
	fn test_resource_id8() {
		const TEST_STR: &str =
			"content/gameplay/japan/ship/destroyer/JSD011_Akizuki_1944/JSD011_Akizuki_1944.mo";
		assert_eq!(ResourceId::new(TEST_STR).value(), 0xa8fa81214165ecb);
	}

	#[test]
	fn test_resource_text() {
		const TEST_STR: &str =
			"content/gameplay/japan/ship/destroyer/JSD011_Akizuki_1944/JSD011_Akizuki_1944.model";
		ResourceId::insert(TEST_STR, 0x0df5a921212a899e);

		assert_eq!(ResourceId(0x0df5a921212a899e).text().unwrap(), TEST_STR);
	}

	#[test]
	fn test_resource_debug() {
		const TEST_STR: &str =
			"content/gameplay/japan/ship/destroyer/JSD011_Akizuki_1944/JSD011_Akizuki_1944.model";
		ResourceId::insert(TEST_STR, 0x0df5a921212a899e);

		assert_eq!(
			format!("{:?}", ResourceId(0x0df5a921212a899e)),
			"\"content/gameplay/japan/ship/destroyer/JSD011_Akizuki_1944/JSD011_Akizuki_1944.model\" (0x0df5a921212a899e)"
		);
	}
}
