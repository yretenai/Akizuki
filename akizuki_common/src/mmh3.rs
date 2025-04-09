// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

#[inline]
pub const fn mmh3_fmix(h: u32) -> u32 {
	let mut h = h;
	h ^= h >> 16;
	h = h.wrapping_mul(0x85ebca6b);
	h ^= h >> 13;
	h = h.wrapping_mul(0xc2b2ae35);
	h ^= h >> 16;
	h
}

#[inline]
pub const fn mmh3_32(bytes: &[u8]) -> u32 {
	const C1: u32 = 0xcc9e2d51;
	const C2: u32 = 0x1b873593;
	const E: u32 = 0xe6546b64;

	let mut h1: u32 = 0;
	let mut k1: u32;

	let mut i = 0;
	while i + 4 <= bytes.len() {
		k1 = u32::from_le_bytes([bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3]]);
		k1 = k1.wrapping_mul(C1);
		k1 = k1.rotate_left(15);
		k1 = k1.wrapping_mul(C2);

		h1 ^= k1;
		h1 = h1.rotate_left(13);
		h1 = h1.wrapping_mul(5).wrapping_add(E);

		i += 4;
	}

	k1 = 0;

	let remainder = bytes.len() - i;

	if remainder > 3 {
		panic!("incomplete vec");
	}

	if remainder > 2 {
		k1 ^= (bytes[i + 2] as u32) << 16;
	}

	if remainder > 1 {
		k1 ^= (bytes[i + 1] as u32) << 8;
	}

	if remainder > 0 {
		k1 ^= bytes[i] as u32;
		k1 = k1.wrapping_mul(C1);
		k1 = k1.rotate_left(15);
		k1 = k1.wrapping_mul(C2);
		h1 ^= k1;
	}

	mmh3_fmix(h1 ^ bytes.len() as u32)
}

#[cfg(test)]
mod tests {
	use super::*;

	#[test]
	fn test_string_id() {
		const TEST_STR: &str = "Akizuki";

		assert_eq!(mmh3_32(TEST_STR.as_ref()), 0x8d949450);
	}

	#[test]
	fn test_string_id4() {
		const TEST_STR: &str = "Akizuki_";

		assert_eq!(mmh3_32(TEST_STR.as_ref()), 0xe344aed1);
	}

	#[test]
	fn test_vec_unsigned() {
		let test_vec: [u8; 4] = [0xff, 0xff, 0xff, 0xff];

		assert_eq!(mmh3_32(&test_vec), 0x76293b50);
	}

	#[test]
	fn test_vec_endianness() {
		let test_vec: [u8; 4] = [0x21, 0x43, 0x65, 0x87];

		assert_eq!(mmh3_32(&test_vec), 0xf55b516b);
	}

	#[test]
	fn test_vec_3() {
		let test_vec: [u8; 3] = [0x21, 0x43, 0x65];

		assert_eq!(mmh3_32(&test_vec), 0x7e4a8634);
	}

	#[test]
	fn test_vec_2() {
		let test_vec: [u8; 2] = [0x21, 0x43];

		assert_eq!(mmh3_32(&test_vec), 0xa0F7b07a);
	}

	#[test]
	fn test_vec_1() {
		let test_vec: [u8; 1] = [0x21];

		assert_eq!(mmh3_32(&test_vec), 0x72661cf4);
	}
}
