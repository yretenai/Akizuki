// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use std::collections::HashMap;
use std::io::SeekFrom::Start;
use std::io::{Cursor, Seek};

use binrw::{BinRead, NullString, VecArgs};
use bytemuck::{Pod, Zeroable};
use glam::IVec2;
use meshopt::ffi;

use crate::bin_wrap::BoundingBox;
use crate::error::{AkizukiError, AkizukiResult};
use crate::format::geometry::*;
use crate::identifiers::{ResourceId, StringId};
use crate::space::vertex::*;
use crate::space::{IndexFormat, VertexFormat};

#[derive(Debug, Clone)]
pub struct GeometryVertexBuffer {
	pub is_skinned: bool,
	pub has_tangent: bool,
	pub vertices: VertexFormat,
}

#[derive(Debug, Clone)]
pub struct GeometryArmorPlate {
	pub thickness: u16,
	pub type_id: u16,
	pub bounding_box: BoundingBox,
	pub vertices: VertexFormat,
}

#[derive(Debug, Clone)]
pub struct GeometryArmor {
	pub name: String,
	pub id: u32,
	pub bounding_box: BoundingBox,
	pub plates: Vec<GeometryArmorPlate>,
}

#[derive(Debug, Clone)]
pub struct GeometryCollisionFace {
	pub inside: Vec<u32>,
	pub outside: Vec<u32>,
	pub unknown: u32,
}

#[derive(Debug, Clone)]
pub struct GeometryCollisionConvexHull {
	pub vertices: VertexFormat,
	pub edges: Vec<IVec2>,
	pub faces: Vec<GeometryCollisionFace>,
}

#[derive(Debug, Clone)]
pub struct GeometryCollision {
	pub name: String,
	pub hulls: Vec<GeometryCollisionConvexHull>,
}

#[derive(Debug, Clone)]
pub struct Geometry {
	pub vertex_mesh_ids: HashMap<StringId, u16>,
	pub vertex_buffer_ids: HashMap<StringId, usize>,
	pub vertex_buffers: Vec<GeometryVertexBuffer>,
	pub index_mesh_ids: HashMap<StringId, u16>,
	pub index_buffer_ids: HashMap<StringId, usize>,
	pub index_buffers: Vec<IndexFormat>,
	pub armor: Option<Vec<GeometryArmor>>,
	pub collision: Option<Vec<GeometryCollision>>,
}

impl Geometry {
	pub fn new(id: ResourceId, buffer: Vec<u8>) -> AkizukiResult<Geometry> {
		let mut reader = Cursor::new(buffer);
		let header = GeometryFileHeader::read_ne(&mut reader)?;

		let mut geometry: Geometry = Geometry {
			vertex_mesh_ids: Default::default(),
			vertex_buffer_ids: Default::default(),
			vertex_buffers: Default::default(),
			index_mesh_ids: Default::default(),
			index_buffer_ids: Default::default(),
			index_buffers: Default::default(),
			armor: read_armor(&mut reader, header.armor_buffer_count, header.armor_buffer_offset)?,
			collision: read_collision(&mut reader, header.collision_buffer_count, header.collision_buffer_offset)?,
		};

		read_names(
			&mut reader,
			&mut geometry.vertex_mesh_ids,
			&mut geometry.vertex_buffer_ids,
			header.vertex_name_count,
			header.vertex_name_offset,
		)?;
		read_names(
			&mut reader,
			&mut geometry.index_mesh_ids,
			&mut geometry.index_buffer_ids,
			header.index_name_count,
			header.index_name_offset,
		)?;
		read_vertex_buffers(id, &mut reader, &mut geometry.vertex_buffers, header.vertex_buffer_count, header.vertex_buffer_offset)?;
		read_index_buffers(id, &mut reader, &mut geometry.index_buffers, header.index_buffer_count, header.index_buffer_offset)?;

		Ok(geometry)
	}
}

fn read_collision(mut reader: &mut Cursor<Vec<u8>>, count: u32, offset: u64) -> AkizukiResult<Option<Vec<GeometryCollision>>> {
	if count == 0 {
		return Ok(None);
	}

	reader.seek(Start(offset))?;
	let mut collisions: Vec<GeometryCollision> = Vec::with_capacity(count as usize);
	let buffer_headers = Vec::<GeometryMiscBufferHeader>::read_ne_args(
		&mut reader,
		VecArgs {
			count: count as usize,
			inner: <_>::default(),
		},
	)?;

	for header in buffer_headers {
		reader.seek(Start(header.relative_position.pos + 8 + header.name_offset))?;
		let name = NullString::read_ne(reader)?.to_string();

		reader.seek(Start(header.relative_position.pos + header.buffer_offset))?;
		let collision = GeometryCollisionConvexHullHeader::read_ne(&mut reader)?;

		collisions.push(GeometryCollision {
			name,
			hulls: collision
				.meshes
				.iter()
				.map(|hull| {
					GeometryCollisionConvexHull {
						vertices: VertexFormat::XYZ(hull.vertices.clone()),
						// i hate that i have to do this rather than trivial copy because of traits being locked to crates
						edges: hull.edges.iter().map(|edge| edge.clone().into()).collect(),
						faces: hull
							.faces
							.iter()
							.map(|face| GeometryCollisionFace {
								inside: face.inside.clone(),
								outside: face.outside.clone(),
								unknown: face.unknown,
							})
							.collect(),
					}
				})
				.collect(),
		})
	}

	Ok(Some(collisions))
}

fn read_armor(mut reader: &mut Cursor<Vec<u8>>, count: u32, offset: u64) -> AkizukiResult<Option<Vec<GeometryArmor>>> {
	if count == 0 {
		return Ok(None);
	}

	reader.seek(Start(offset))?;
	let mut armors: Vec<GeometryArmor> = Vec::with_capacity(count as usize);
	let buffer_headers = Vec::<GeometryMiscBufferHeader>::read_ne_args(
		&mut reader,
		VecArgs {
			count: count as usize,
			inner: <_>::default(),
		},
	)?;

	for header in buffer_headers {
		reader.seek(Start(header.relative_position.pos + 8 + header.name_offset))?;
		let name = NullString::read_ne(reader)?.to_string();

		reader.seek(Start(header.relative_position.pos + header.buffer_offset))?;
		let armor = GeometryArmorSurfaceHeader::read_ne(&mut reader)?;

		armors.push(GeometryArmor {
			name,
			id: armor.id,
			bounding_box: armor.bounding_box,
			plates: armor
				.plates
				.iter()
				.map(move |plate| GeometryArmorPlate {
					thickness: plate.thickness,
					type_id: plate.type_id,
					bounding_box: plate.bounding_box,
					vertices: VertexFormat::XYZN(plate.vertices.clone()),
				})
				.collect(),
		})
	}

	Ok(Some(armors))
}

fn read_vertex_buffers(
	resource_id: ResourceId,
	mut reader: &mut Cursor<Vec<u8>>,
	buffers: &mut Vec<GeometryVertexBuffer>,
	count: u32,
	offset: u64,
) -> AkizukiResult<()> {
	if count == 0 {
		return Ok(());
	}

	// note: this allocates two or three full vectors, can we reduce that somehow?

	reader.seek(Start(offset))?;
	let buffer_headers = Vec::<GeometryVertexBufferHeader>::read_ne_args(
		&mut reader,
		VecArgs {
			count: count as usize,
			inner: <_>::default(),
		},
	)?;

	for header in buffer_headers {
		reader.seek(Start(header.relative_position.pos + 8 + header.vertex_format_offset))?;
		let format = NullString::read_ne(reader)?.to_string();

		reader.seek(Start(header.relative_position.pos + header.buffer_offset))?;
		let buffer: Vec<u8> = {
			let buffer = Vec::<u8>::read_ne_args(
				&mut reader,
				VecArgs {
					count: header.buffer_length as usize,
					inner: <_>::default(),
				},
			)?;
			let magic = *bytemuck::from_bytes::<u32>(&buffer);
			if magic == 0x44434e45 {
				// meshopt'd
				let vertex_count = *bytemuck::from_bytes::<u32>(&buffer[4..]) as usize;
				let mut new_buffer = vec![0u8; vertex_count * header.vertex_stride as usize];

				let result_code = unsafe {
					ffi::meshopt_decodeVertexBuffer(
						new_buffer.as_mut_ptr().cast(),
						vertex_count,
						header.vertex_stride as usize,
						buffer[8..].as_ptr(),
						buffer[8..].len(),
					)
				};

				if result_code != 0 { Err(meshopt::error::Error::Native(result_code)) } else { Ok(new_buffer) }
			} else {
				Ok(buffer)
			}
		}?;

		// todo: macro this
		// also should we do this here?
		let vertices = match format.as_str() {
			"set3/xyzpc" => VertexFormat::XYZ(decode_vertex_buffer::<VertexXYZ>(resource_id, format, buffer, header.vertex_stride)?),
			"set3/xyznpc" => VertexFormat::XYZN(decode_vertex_buffer::<VertexXYZN>(resource_id, format, buffer, header.vertex_stride)?),
			"set3/xyznuvpc" => {
				VertexFormat::XYZNUV(decode_vertex_buffer::<VertexXYZNUV>(resource_id, format, buffer, header.vertex_stride)?)
			}
			"set3/xyznuv2iiiwwtbpc" => VertexFormat::XYZNUV2IIIWWTB(decode_vertex_buffer::<VertexXYZNUV2IIIWWTB>(
				resource_id,
				format,
				buffer,
				header.vertex_stride,
			)?),
			"set3/xyznuv2tbpc" => {
				VertexFormat::XYZNUV2TB(decode_vertex_buffer::<VertexXYZNUV2TB>(resource_id, format, buffer, header.vertex_stride)?)
			}
			"set3/xyznuv2tbipc" => {
				VertexFormat::XYZNUV2TBI(decode_vertex_buffer::<VertexXYZNUV2TBI>(resource_id, format, buffer, header.vertex_stride)?)
			}
			"set3/xyznuviiiwwpc" => {
				VertexFormat::XYZNUVIIIWW(decode_vertex_buffer::<VertexXYZNUVIIIWW>(resource_id, format, buffer, header.vertex_stride)?)
			}
			"set3/xyznuviiiwwr" => {
				VertexFormat::XYZNUVIIIWWR(decode_vertex_buffer::<VertexXYZNUVIIIWWR>(resource_id, format, buffer, header.vertex_stride)?)
			}
			"set3/xyznuviiiwwtbpc" => {
				VertexFormat::XYZNUVIIIWWTB(decode_vertex_buffer::<VertexXYZNUVIIIWWTB>(resource_id, format, buffer, header.vertex_stride)?)
			}
			"set3/xyznuvrpc" => {
				VertexFormat::XYZNUVR(decode_vertex_buffer::<VertexXYZNUVR>(resource_id, format, buffer, header.vertex_stride)?)
			}
			"set3/xyznuvtbpc" => {
				VertexFormat::XYZNUVTB(decode_vertex_buffer::<VertexXYZNUVTB>(resource_id, format, buffer, header.vertex_stride)?)
			}
			"set3/xyznuvtbipc" => {
				VertexFormat::XYZNUVTBI(decode_vertex_buffer::<VertexXYZNUVTBI>(resource_id, format, buffer, header.vertex_stride)?)
			}
			"set3/xyznuvtboi" => {
				VertexFormat::XYZNUVTBOI(decode_vertex_buffer::<VertexXYZNUVTBOI>(resource_id, format, buffer, header.vertex_stride)?)
			}
			_ => return Err(AkizukiError::UnrecognizedVertexFormat(resource_id, format)),
		};

		buffers.push(GeometryVertexBuffer {
			is_skinned: header.is_skinned.into(),
			has_tangent: header.has_tangent.into(),
			vertices,
		})
	}

	Ok(())
}

pub fn decode_vertex_buffer<T: Pod + Zeroable>(id: ResourceId, format: String, buffer: Vec<u8>, stride: u16) -> AkizukiResult<Vec<T>> {
	if stride as usize != size_of::<T>() {
		return Err(AkizukiError::UnrecognizedVertexFormat(id, format));
	}

	let vec = bytemuck::try_cast_slice::<u8, T>(&buffer).map_err(|err| AkizukiError::PodCast(err))?;

	// !! hard clone !!
	Ok(vec.into())
}

fn read_index_buffers(
	resource_id: ResourceId,
	mut reader: &mut Cursor<Vec<u8>>,
	buffers: &mut Vec<IndexFormat>,
	count: u32,
	offset: u64,
) -> AkizukiResult<()> {
	if count == 0 {
		return Ok(());
	}

	reader.seek(Start(offset))?;
	let buffer_headers = Vec::<GeometryIndexBufferHeader>::read_ne_args(
		&mut reader,
		VecArgs {
			count: count as usize,
			inner: <_>::default(),
		},
	)?;

	for header in buffer_headers {
		reader.seek(Start(header.relative_position.pos + header.buffer_offset))?;
		let count = header.buffer_length as usize / header.index_stride as usize;
		buffers.push(match header.index_stride {
			2 => IndexFormat::U16(Vec::<u16>::read_ne_args(
				&mut reader,
				VecArgs {
					count,
					inner: <_>::default(),
				},
			)?),
			4 => IndexFormat::U32(Vec::<u32>::read_ne_args(
				&mut reader,
				VecArgs {
					count,
					inner: <_>::default(),
				},
			)?),
			_ => return Err(AkizukiError::UnrecognizedIndexFormat(resource_id, header.index_stride)),
		})
	}

	Ok(())
}

fn read_names(
	mut reader: &mut Cursor<Vec<u8>>,
	mesh_ids: &mut HashMap<StringId, u16>,
	buffer_ids: &mut HashMap<StringId, usize>,
	count: u32,
	offset: u64,
) -> AkizukiResult<()> {
	if count == 0 {
		return Ok(());
	}

	reader.seek(Start(offset))?;
	let names = Vec::<GeometryNameRaw>::read_ne_args(
		&mut reader,
		VecArgs {
			count: count as usize,
			inner: <_>::default(),
		},
	)?;

	for name in names {
		mesh_ids.insert(name.name, name.mesh_id);
		buffer_ids.insert(name.name, name.buffer_id as usize);
	}

	Ok(())
}
