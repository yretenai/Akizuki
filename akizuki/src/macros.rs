// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

#[macro_export]
macro_rules! bigworld_table_check {
	($name:ident, $version:path, $reader:ident, $header:ident) => {
		if $name::is_valid_for($header.id, $header.version) {
			return Ok($version($name::new($reader)?.into()));
		}
	};
}

#[macro_export]
macro_rules! bigworld_table_version {
	($name:ident, $header:ident) => {
		if $name::is_valid_for($header.id, $header.version) {
			return true;
		}
	};
}

#[macro_export]
macro_rules! bigworld_read_array {
	($reader:ident, $header:ident, $name:ident, $count:ident, $offset:ident, $type:ident) => {
		$reader.seek(Start($header.relative_position.pos + $header.$offset))?;
		let $name = Vec::<$type>::read_ne_args(
			$reader,
			VecArgs {
				count: $header.$count as usize,
				inner: <_>::default(),
			},
		)?;
	};
}
