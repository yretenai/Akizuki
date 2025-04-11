// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

extern crate proc_macro;

use akizuki_common::mmh3::mmh3_32;

use proc_macro::TokenStream;
use proc_macro2::Ident;
use quote::quote;
use syn::parse::{Parse, ParseStream};
use syn::{DeriveInput, LitInt, LitStr, Path, Result, Token, parse_macro_input};

#[proc_macro]
/// construct a string id at compile time based on the given string.
///
/// akizuki_id("string to hash") -> StringId
pub fn akizuki_id(input: TokenStream) -> TokenStream {
	let input = parse_macro_input!(input as LitStr);
	let str = input.value();
	let hash = mmh3_32(str.as_ref());

	TokenStream::from(quote! {
		StringId(#hash)
	})
}

#[proc_macro]
/// generates a block of code to check if a version is valid, and constructs that version.
///
/// bigworld_table_version(path::to:table, header, reader)
pub fn bigworld_table_version(input: TokenStream) -> TokenStream {
	let input = parse_macro_input!(input as BigWorldTableVersionsParams);
	let name = input.0;
	let header = input.1;
	let reader = input.2;
	TokenStream::from(quote! {
		if #name::is_valid_for(&#header.id, #header.version) {
			return Ok(#name::new(#reader)?.into());
		}
	})
}

#[proc_macro]
/// generates a block of code to check if a version is valid, returning bool true if so.
///
/// bigworld_table_check(path::to:table, header)
pub fn bigworld_table_check(input: TokenStream) -> TokenStream {
	let input = parse_macro_input!(input as BigWorldTableCheckParams);
	let name = input.0;
	let header = input.1;
	TokenStream::from(quote! {
		if #name::is_valid_for(&#header.id, #header.version) {
			return true;
		}
	})
}

#[proc_macro]
/// checks if the table is supported, if so constructs and pushes the table states and continues.
/// if not, pushes a TableError to the states.
///
/// bigworld_table_branch(path::to:table, construct_table, tables, table_states, header, reader)
pub fn bigworld_table_branch(input: TokenStream) -> TokenStream {
	let input = parse_macro_input!(input as BigWorldTableBranchParams);
	let target = input.0;
	let func = input.1;
	let tables = input.2;
	let states = input.3;
	let header = input.4;
	let reader = input.5;

	TokenStream::from(quote! {
		if #target::is_supported(&#header) {
			#states.push(None);
			#tables.push(#func::<#target>(#reader, #header)?);
			continue;
		}

		warn!("table {:?} (version {:08x}) is not implememented", table_header.id, table_header.version);

		#states.push(Some(TableError::UnsupportedTableVersion(
			#header.id,
			#header.version,
		)));
	})
}

#[proc_macro_derive(BigWorldTable, attributes(table))]
/// generates a is_valid_for function for the given TableName, to be used by the super implementation.
/// usually accessed via [bigworld_table_version] and [bigworld_table_check].
///
/// #\[derive(BigWorldTable)]
/// #\[table(TableName, versions...)]
pub fn bigworld_table_derive(input: TokenStream) -> TokenStream {
	let input = parse_macro_input!(input as DeriveInput);
	let name = &input.ident;
	let attribute = input
		.attrs
		.iter()
		.find(|attr| attr.path().is_ident("table"))
		.expect("table attribute required for deriving BigWorldTable");
	let params: BigWorldTableParams = attribute.parse_args().expect("invalid params");
	let hash = mmh3_32(params.0.to_string().as_ref());

	let expanded = match params.1.as_slice() {
		[] => panic!("need at least one version"),
		[single] => quote! {
			impl #name {
				pub fn is_valid_for(hash: &StringId, version: u32) -> bool {
					hash == &StringId(#hash) && version == #single
				}
			}
		},
		multiple => {
			let version_checks = multiple.iter().map(|v| quote! { version == #v }).collect::<Vec<_>>();
			quote! {
				impl #name {
					pub fn is_valid_for(hash: &StringId, version: u32) -> bool {
						if hash == &StringId(#hash) {
							return false;
						}

						#(#version_checks)||*
					}
				}
			}
		}
	};

	expanded.into()
}

struct BigWorldTableVersionsParams(Path, Ident, Ident);
impl Parse for BigWorldTableVersionsParams {
	fn parse(content: ParseStream) -> Result<Self> {
		let name: Path = content.parse()?;
		content.parse::<Token![,]>()?;
		let reader: Ident = content.parse()?;
		content.parse::<Token![,]>()?;
		let header: Ident = content.parse()?;
		Ok(BigWorldTableVersionsParams(name, header, reader))
	}
}

struct BigWorldTableCheckParams(Path, Ident);
impl Parse for BigWorldTableCheckParams {
	fn parse(content: ParseStream) -> Result<Self> {
		let name: Path = content.parse()?;
		content.parse::<Token![,]>()?;
		let header: Ident = content.parse()?;
		Ok(BigWorldTableCheckParams(name, header))
	}
}

struct BigWorldTableBranchParams(Path, Path, Ident, Ident, Ident, Ident);
impl Parse for BigWorldTableBranchParams {
	fn parse(content: ParseStream) -> Result<Self> {
		let target: Path = content.parse()?;
		content.parse::<Token![,]>()?;
		let func: Path = content.parse()?;
		content.parse::<Token![,]>()?;
		let tables: Ident = content.parse()?;
		content.parse::<Token![,]>()?;
		let states: Ident = content.parse()?;
		content.parse::<Token![,]>()?;
		let header: Ident = content.parse()?;
		content.parse::<Token![,]>()?;
		let reader: Ident = content.parse()?;
		Ok(BigWorldTableBranchParams(target, func, tables, states, header, reader))
	}
}

struct BigWorldTableParams(Ident, Vec<LitInt>);
impl Parse for BigWorldTableParams {
	fn parse(content: ParseStream) -> Result<Self> {
		let name: Ident = content.parse()?;
		let mut versions = Vec::<LitInt>::new();
		if content.peek(Token![,]) {
			content.parse::<Token![,]>()?;

			let version: LitInt = content.parse()?;
			versions.push(version);

			while !content.is_empty() {
				if content.peek(Token![,]) {
					content.parse::<Token![,]>()?;
				}
			}
		}

		Ok(BigWorldTableParams(name, versions))
	}
}
