// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

extern crate proc_macro;

use akizuki_common::mmh3::mmh3_32;
use proc_macro::TokenStream;
use proc_macro2::Ident;
use quote::quote;
use syn::parse::{Parse, ParseStream};
use syn::{DeriveInput, LitByteStr, LitInt, Result, Token, parse_macro_input};

#[proc_macro]
pub fn akizuki_id(input: TokenStream) -> TokenStream {
	let input = parse_macro_input!(input as LitByteStr);
	let bytes = input.value();

	let hash = mmh3_32(&bytes);

	TokenStream::from(quote! {
		#hash as u32
	})
}

struct BigWorldTableParams(Ident, Vec<LitInt>);
impl Parse for BigWorldTableParams {
	fn parse(content: ParseStream) -> Result<Self> {
		let name: Ident = content.parse()?;
		let mut versions = Vec::<LitInt>::new();
		if content.peek(Token![,]) {
			content.parse::<Token![,]>()?;

			while !content.is_empty() {
				let version: LitInt = content.parse()?;
				versions.push(version);

				if content.peek(Token![,]) {
					content.parse::<Token![,]>()?;
				}
			}
		}

		Ok(BigWorldTableParams(name, versions))
	}
}

#[proc_macro_derive(BigWorldTable, attributes(table))]
pub fn bigworld_table_derive(input: TokenStream) -> TokenStream {
	let input = parse_macro_input!(input as DeriveInput);
	let name = &input.ident;
	let attribute = input.attrs.iter().find(|attr| attr.path().is_ident("table")).expect("table attribute required for deriving BigWorldTable");
	let params: BigWorldTableParams = attribute.parse_args().expect("invalid params");
	let hash = mmh3_32(params.0.to_string().as_ref());

	let expanded = match params.1.as_slice() {
		[] => quote! {
			impl TableRecord for #name {
				fn is_valid_for(hash: &StringId, _version: u32) -> bool {
					const _table_id: StringId = StringId(#hash);
					hash.eq(&_table_id)
				}

				fn create(mut reader: &mut Cursor<&[u8]>) -> AkizukiResult<BigWorldTableRecord> {
					Ok(BigWorldTableRecord::#name(Self::new(reader)?))
				}
			}
		},
		[single] => quote! {
			impl TableRecord for #name {
				fn is_valid_for(hash: &StringId, version: u32) -> bool {
					const _table_id: StringId = StringId(#hash);
					hash.eq(&_table_id) && version == #single
				}

				fn create(mut reader: &mut Cursor<&[u8]>) -> AkizukiResult<BigWorldTableRecord> {
					Ok(BigWorldTableRecord::#name(Self::new(reader)?))
				}
			}
		},
		multiple => {
			let version_checks = multiple.iter().map(|v| quote! { version == #v }).collect::<Vec<_>>();
			quote! {
				impl TableRecord for #name {
					fn is_valid_for(hash: &StringId, version: u32) -> bool {
						const _table_id: StringId = StringId(#hash);
						if !hash.eq(&_table_id) {
							return false;
						}

						#(#version_checks)||*
					}
				}

				fn create(mut reader: &mut Cursor<&[u8]>) -> AkizukiResult<BigWorldTableRecord> {
					Ok(BigWorldTableRecord::#name(Self::new(reader)?))
				}
			}
		}
	};

	expanded.into()
}
