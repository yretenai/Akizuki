// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

extern crate proc_macro;

use akizuki_common::mmh3::mmh3_32;

use proc_macro::TokenStream;
use quote::quote;
use syn::parse::{Parse, ParseStream};
use syn::{DeriveInput, LitInt, LitStr, Result, Token, parse_macro_input};

#[proc_macro]
/// construct a string id at compile time based on the given string.
///
/// akizuki_id!("string to hash") -> StringId
pub fn akizuki_id(input: TokenStream) -> TokenStream {
	let input = parse_macro_input!(input as LitStr);
	let str = input.value();
	let hash = mmh3_32(str.as_ref());

	TokenStream::from(quote! {
		StringId(#hash)
	})
}

#[proc_macro]
/// construct a resource id at compile time based on the given string.
///
/// akizuki_resource!("resource path to hash") -> ResourceId
pub fn akizuki_resource(input: TokenStream) -> TokenStream {
	let input = parse_macro_input!(input as LitStr);
	let str = input.value();
	let hash = cityhasher::hash::<u64>(str);

	TokenStream::from(quote! {
		ResourceId(#hash)
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
	let attribute = input.attrs.iter().find(|attr| attr.path().is_ident("table"));
	let attribute = attribute.expect("table attribute required for deriving BigWorldTable");
	let params: BigWorldTableParams = attribute.parse_args().expect("invalid params");
	let hash = mmh3_32(params.0.value().as_ref());

	let expanded = match params.1.as_slice() {
		[] => panic!("need at least one version"),
		[single] => quote! {
			impl #name {
				pub fn is_valid_for(hash: StringId, version: u32) -> bool {
					hash == #hash && version == #single
				}
			}
		},
		multiple => {
			let version_checks = multiple.iter().map(|v| quote! { version == #v }).collect::<Vec<_>>();
			quote! {
				impl #name {
					pub fn is_valid_for(hash: StringId, version: u32) -> bool {
						if hash == #hash {
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

struct BigWorldTableParams(LitStr, Vec<LitInt>);
impl Parse for BigWorldTableParams {
	fn parse(content: ParseStream) -> Result<Self> {
		let name: LitStr = content.parse()?;
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
