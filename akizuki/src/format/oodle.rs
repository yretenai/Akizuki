// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

#![allow(non_snake_case)]

use dlopen2::wrapper::{Container, WrapperApi};
use std::ffi::c_void;
use std::ptr::{null, null_mut};

#[repr(C)]
pub struct BC7PrepHeader {
	pub version: u32,
	pub flags: u32,
	pub mode_counts: [u32; 10],
}

#[derive(WrapperApi)]
pub struct Oodle {
	OodleLZDecoder_MemorySizeNeeded: unsafe extern "C" fn(compressor: i32, size: i64) -> i32,
	#[allow(clippy::too_many_arguments)]
	OodleLZ_Decompress: unsafe extern "C" fn(src_buf: *const u8, src_size: i64, raw_buf: *mut u8, raw_size: i64, fuzz_safe: bool, check_crc: bool, verbosity: i32, dec_buf_base: *mut u8, dec_buf_size: i64, fp_callback: *const c_void, callback_data: *const c_void, decoder_memory: *mut u8, decoder_memory_size: i64, thread_phase: i32) -> i64,
}

#[derive(WrapperApi)]
struct OodleTex {
	OodleTexRT_BC7Prep_ReadHeader: unsafe extern "C" fn(header: *const BC7PrepHeader, num_blocks: *mut i64, payload_size: *mut i64) -> i32,
	OodleTexRT_BC7Prep_MinDecodeScratchSize: unsafe extern "C" fn(num_blocks: i64) -> i64,
	#[allow(clippy::too_many_arguments)]
	OodleTexRT_BC7Prep_Decode: unsafe extern "C" fn(output_buf: *const u8, output_size: i64, bc7_buf: *const u8, bc7_size: i64, header: *const BC7PrepHeader, flags: u32, scratch_buf: *const u8, scratch_size: i64) -> i64,
}

#[cfg(target_os = "linux")]
const OODLE_NAME: &str = "liboo2core.so";
#[cfg(target_os = "windows")]
const OODLE_NAME: &str = "liboo2core.dll";
#[cfg(target_os = "macos")]
const OODLE_NAME: &str = "liboo2core.dylib";
#[cfg(not(any(target_os = "linux", target_os = "windows", target_os = "macos")))]
const OODLE_NAME: &str = "liboo2core";

#[cfg(target_os = "linux")]
#[allow(dead_code)]
const OODLETEX_NAME: &str = "liboo2texrt.so";
#[cfg(target_os = "windows")]
const OODLETEX_NAME: &str = "liboo2texrt.dll";
#[cfg(target_os = "macos")]
const OODLETEX_NAME: &str = "liboo2texrt.dylib";
#[cfg(not(any(target_os = "linux", target_os = "windows", target_os = "macos")))]
const OODLETEX_NAME: &str = "liboo2texrt";

pub fn decompress_oodle_data(compressed: &[u8], uncompressed: &mut [u8]) -> Result<usize, i64> {
	unsafe {
		let Ok(oodle) = Container::<Oodle>::load(OODLE_NAME) else { return Err(-1) };

		let memorySize = oodle.OodleLZDecoder_MemorySizeNeeded(-1, -1);
		let mut scratch: Vec<u8> = vec![0; memorySize as usize];
		let in_ref = compressed.as_ptr();
		let out_ref = uncompressed.as_mut_ptr();
		let scratch_ref = scratch.as_mut_ptr();
		let result = oodle.OodleLZ_Decompress(in_ref, compressed.len() as i64, out_ref, uncompressed.len() as i64, true, false, 0, null_mut(), 0, null(), null(), scratch_ref, scratch.len() as i64, 3);
		if result > 0 { Ok(result as usize) } else { Err(result) }
	}
}

#[allow(dead_code)]
pub fn decompress_oodle_bc7(header: &BC7PrepHeader, compressed: &[u8], uncompressed: &mut [u8]) -> Result<usize, i64> {
	unsafe {
		let Ok(oodle) = Container::<OodleTex>::load(OODLETEX_NAME) else { return Err(-1) };

		let mut blocks: i64 = 0;
		let mut payload_size: i64 = 0;
		if oodle.OodleTexRT_BC7Prep_ReadHeader(header, &mut blocks, &mut payload_size) < 0 {
			return Err(-1);
		}

		if payload_size > uncompressed.len() as i64 {
			return Err(-1);
		}

		let memorySize = oodle.OodleTexRT_BC7Prep_MinDecodeScratchSize(blocks);
		let mut scratch: Vec<u8> = vec![0; memorySize as usize];
		let in_ref = compressed.as_ptr();
		let out_ref = uncompressed.as_mut_ptr();
		let scratch_ref = scratch.as_mut_ptr();
		let result = oodle.OodleTexRT_BC7Prep_Decode(out_ref, uncompressed.len() as i64, in_ref, compressed.len() as i64, header, 0, scratch_ref, scratch.len() as i64);
		if result > 0 { Ok(result as usize) } else { Err(result) }
	}
}
