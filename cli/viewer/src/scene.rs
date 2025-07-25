// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use std::collections::HashMap;
use std::sync::Arc;

use akizuki::identifiers::{ResourceId, StringId};
use akizuki::table::material::MaterialPrototypeVersion;
use glam::Mat4;

#[derive(Default, Debug)]
pub struct Scene {
	pub nodes: Vec<Node>,
	pub textures: HashMap<ResourceId, Arc<wgpu::Texture>>,
	pub meshes: HashMap<(ResourceId, StringId), Arc<Mesh>>,
	pub frag_shaders: HashMap<String, FragShader>,
	pub vert_shaders: HashMap<String, VertShader>,
}

#[derive(Debug)]
pub struct FragShader {
	pub shader: Arc<wgpu::ShaderModule>,
	pub texture_layout: HashMap<StringId, u64>,
	pub bind_data_layout: HashMap<StringId, u64>,
	pub bind_size: u64,
}

#[derive(Debug)]
pub struct VertShader {
	pub shader: Arc<wgpu::ShaderModule>,
	pub texture_layout: HashMap<StringId, u64>,
	pub bind_data_layout: HashMap<StringId, u64>,
	pub vertex_stride: u64,
	pub vertex_attributes: Vec<wgpu::VertexAttribute>,
}

#[derive(Debug)]
pub struct Node {
	pub name: String,
	pub mesh: Option<(ResourceId, StringId)>,
	pub local_matrix: Mat4,
	pub children: Vec<Node>,
}

#[derive(Debug)]
pub struct Mesh {
	pub vertex_buffer: wgpu::Buffer,
	pub index_buffer: wgpu::Buffer,
	pub submeshes: Vec<Submesh>,
}

#[derive(Debug)]
pub struct Submesh {
	pub material: Arc<Material>,
	pub first_vertex: i32,
	pub first_index: u32,
	pub last_index: u32,
}

#[derive(Debug)]
pub struct Material {
	pub slot: MaterialPrototypeVersion,
	pub vert_shader: String,
	pub frag_shader: String,
	pub bind_group_layout: wgpu::BindGroupLayout,
	pub pipeline_layout: wgpu::PipelineLayout,
	pub pipeline: Arc<wgpu::RenderPipeline>,
}

// todo: impl
