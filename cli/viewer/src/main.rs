// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

use std::sync::Arc;
use std::time::Instant;

use bytemuck::{Pod, Zeroable};
use log::LevelFilter;
use pollster::block_on;
use wgpu::util::DeviceExt;
use wgpu::{CommandEncoder, include_wgsl};
use winit::application::ApplicationHandler;
use winit::dpi::{LogicalSize, PhysicalSize};
use winit::event_loop::{ActiveEventLoop, ControlFlow};
use winit::keyboard::{Key, NamedKey};
use winit::{event::WindowEvent, event_loop::EventLoop, window::Window};

#[repr(C)]
#[derive(Clone, Copy, Pod, Zeroable)]
struct GpuVertex {
	position: [f32; 4],
	color: [f32; 4],
}

fn vertex(pos: [f32; 3], color: [f32; 4]) -> GpuVertex {
	GpuVertex {
		position: [pos[0], pos[1], pos[2], 1.0],
		color,
	}
}

fn create_tri() -> (Vec<GpuVertex>, Vec<u16>) {
	let vertex = vec![
		vertex([-1.0, -1.0, 0.0], [1.0, 0.0, 0.0, 1.0]),
		vertex([0.0, 1.0, 0.0], [0.0, 1.0, 0.0, 1.0]),
		vertex([1.0, -1.0, 0.0], [0.0, 0.0, 1.0, 1.0]),
	];

	let index = vec![2, 1, 0];

	(vertex, index)
}

struct AkizukiViewer {
	vertex_buf: wgpu::Buffer,
	index_buf: wgpu::Buffer,
	index_count: usize,
	bind_group_layout: wgpu::BindGroupLayout,
	pipeline: wgpu::RenderPipeline,
	time: f32,
	last_render: Instant,
}

struct AppWindow {
	device: wgpu::Device,
	queue: wgpu::Queue,
	window: Arc<Window>,
	surface_desc: wgpu::SurfaceConfiguration,
	surface: wgpu::Surface<'static>,
	_hidpi_factor: f64,
	context: AkizukiViewer,
}

#[derive(Default)]
struct App {
	window: Option<AppWindow>,
}

impl AkizukiViewer {
	fn new(caps: &wgpu::SurfaceCapabilities, device: &wgpu::Device) -> Self {
		let vertex_size = size_of::<GpuVertex>();
		let (vertex_data, index_data) = create_tri();

		let vertex_buf = device.create_buffer_init(&wgpu::util::BufferInitDescriptor {
			label: Some("Vertex Buffer"),
			contents: bytemuck::cast_slice(&vertex_data),
			usage: wgpu::BufferUsages::VERTEX,
		});

		let index_buf = device.create_buffer_init(&wgpu::util::BufferInitDescriptor {
			label: Some("Index Buffer"),
			contents: bytemuck::cast_slice(&index_data),
			usage: wgpu::BufferUsages::INDEX,
		});

		let bind_group_layout = device.create_bind_group_layout(&wgpu::BindGroupLayoutDescriptor {
			label: None,
			entries: &[
				wgpu::BindGroupLayoutEntry {
					binding: 0,
					visibility: wgpu::ShaderStages::VERTEX,
					ty: wgpu::BindingType::Buffer {
						ty: wgpu::BufferBindingType::Uniform,
						has_dynamic_offset: false,
						min_binding_size: wgpu::BufferSize::new(4),
					},
					count: None,
				},
				wgpu::BindGroupLayoutEntry {
					binding: 1,
					visibility: wgpu::ShaderStages::FRAGMENT,
					ty: wgpu::BindingType::Buffer {
						ty: wgpu::BufferBindingType::Uniform,
						has_dynamic_offset: false,
						min_binding_size: wgpu::BufferSize::new(4),
					},
					count: None,
				},
			],
		});

		let pipeline_layout = device.create_pipeline_layout(&wgpu::PipelineLayoutDescriptor {
			label: None,
			bind_group_layouts: &[&bind_group_layout],
			push_constant_ranges: &[],
		});

		let vertex_buffers = [wgpu::VertexBufferLayout {
			array_stride: vertex_size as wgpu::BufferAddress,
			step_mode: wgpu::VertexStepMode::Vertex,
			attributes: &[
				wgpu::VertexAttribute {
					format: wgpu::VertexFormat::Float32x4,
					offset: 0,
					shader_location: 0,
				},
				wgpu::VertexAttribute {
					format: wgpu::VertexFormat::Float32x4,
					offset: 4 * 4,
					shader_location: 1,
				},
			],
		}];

		let shader = device.create_shader_module(include_wgsl!("shader.wgsl"));
		let swapchain_format = caps.formats[0];

		let pipeline = device.create_render_pipeline(&wgpu::RenderPipelineDescriptor {
			label: None,
			layout: Some(&pipeline_layout),
			vertex: wgpu::VertexState {
				module: &shader,
				entry_point: Some("vs_main"),
				compilation_options: Default::default(),
				buffers: &vertex_buffers,
			},
			fragment: Some(wgpu::FragmentState {
				module: &shader,
				entry_point: Some("fs_main"),
				compilation_options: Default::default(),
				targets: &[Some(swapchain_format.into())],
			}),
			primitive: wgpu::PrimitiveState {
				cull_mode: Some(wgpu::Face::Back),
				..Default::default()
			},
			depth_stencil: None,
			multisample: wgpu::MultisampleState::default(),
			multiview: None,
			cache: None,
		});

		AkizukiViewer {
			vertex_buf,
			index_buf,
			index_count: index_data.len(),
			bind_group_layout,
			pipeline,
			time: 0.0,
			last_render: Instant::now(),
		}
	}

	fn update(&mut self, delta_time: f32) {
		self.time += delta_time;
	}

	fn render(&mut self, view: &wgpu::TextureView, device: &wgpu::Device, encoder: &mut CommandEncoder) {
		let uniform_buf = device.create_buffer_init(&wgpu::util::BufferInitDescriptor {
			label: Some("Time Buffer"),
			contents: bytemuck::bytes_of(&self.time),
			usage: wgpu::BufferUsages::UNIFORM | wgpu::BufferUsages::COPY_DST,
		});

		let bind_group = device.create_bind_group(&wgpu::BindGroupDescriptor {
			layout: &self.bind_group_layout,
			entries: &[
				wgpu::BindGroupEntry {
					binding: 0,
					resource: uniform_buf.as_entire_binding(),
				},
				wgpu::BindGroupEntry {
					binding: 1,
					resource: uniform_buf.as_entire_binding(),
				},
			],
			label: None,
		});

		{
			let mut rpass = encoder.begin_render_pass(&wgpu::RenderPassDescriptor {
				label: None,
				color_attachments: &[Some(wgpu::RenderPassColorAttachment {
					view,
					resolve_target: None,
					ops: wgpu::Operations {
						load: wgpu::LoadOp::Clear(wgpu::Color::BLACK),
						store: wgpu::StoreOp::Store,
					},
				})],
				depth_stencil_attachment: None,
				timestamp_writes: None,
				occlusion_query_set: None,
			});
			rpass.set_pipeline(&self.pipeline);
			rpass.set_bind_group(0, &bind_group, &[]);
			rpass.set_index_buffer(self.index_buf.slice(..), wgpu::IndexFormat::Uint16);
			rpass.set_vertex_buffer(0, self.vertex_buf.slice(..));
			rpass.draw_indexed(0..self.index_count as u32, 0, 0..1);
		}
	}
}

impl AppWindow {
	fn new(event_loop: &ActiveEventLoop) -> Self {
		let instance = wgpu::Instance::new(&wgpu::InstanceDescriptor {
			backends: wgpu::Backends::PRIMARY,
			..Default::default()
		});

		let window = {
			let version = env!("CARGO_PKG_VERSION");

			let size = LogicalSize::new(1280.0, 720.0);

			let attributes = Window::default_attributes().with_inner_size(size).with_title(format!("Akizuki {version}"));
			Arc::new(event_loop.create_window(attributes).unwrap())
		};

		let size = window.inner_size();
		let hidpi_factor = window.scale_factor();
		let surface = instance.create_surface(window.clone()).unwrap();

		let adapter = block_on(instance.request_adapter(&wgpu::RequestAdapterOptions {
			power_preference: wgpu::PowerPreference::HighPerformance,
			compatible_surface: Some(&surface),
			force_fallback_adapter: false,
		}))
		.unwrap();

		let (device, queue) = block_on(adapter.request_device(&wgpu::DeviceDescriptor {
			label: None,
			required_features: wgpu::Features::TEXTURE_COMPRESSION_BC,
			required_limits: wgpu::Limits::default(),
			memory_hints: wgpu::MemoryHints::Performance,
			trace: wgpu::Trace::Off,
		}))
		.unwrap();

		// Set up swap chain
		let surface_desc = create_surface_desc(&size);

		surface.configure(&device, &surface_desc);

		let context = AkizukiViewer::new(&surface.get_capabilities(&adapter), &device);

		Self {
			device,
			queue,
			window,
			surface_desc,
			surface,
			_hidpi_factor: hidpi_factor,
			context,
		}
	}
}

fn create_surface_desc(size: &PhysicalSize<u32>) -> wgpu::SurfaceConfiguration {
	wgpu::SurfaceConfiguration {
		usage: wgpu::TextureUsages::RENDER_ATTACHMENT,
		format: wgpu::TextureFormat::Bgra8UnormSrgb,
		width: size.width,
		height: size.height,
		present_mode: wgpu::PresentMode::Immediate,
		desired_maximum_frame_latency: 1,
		alpha_mode: wgpu::CompositeAlphaMode::Opaque,
		view_formats: vec![wgpu::TextureFormat::Bgra8Unorm],
	}
}

impl ApplicationHandler for App {
	fn resumed(&mut self, event_loop: &ActiveEventLoop) {
		self.window = Some(AppWindow::new(event_loop));
	}

	fn window_event(&mut self, event_loop: &ActiveEventLoop, _window_id: winit::window::WindowId, event: WindowEvent) {
		let window = self.window.as_mut().unwrap();

		match &event {
			WindowEvent::Resized(size) => {
				window.surface_desc = create_surface_desc(size);

				window.surface.configure(&window.device, &window.surface_desc);
			}
			WindowEvent::CloseRequested => event_loop.exit(),
			WindowEvent::KeyboardInput {
				event,
				..
			} => {
				if let Key::Named(NamedKey::Escape) = event.logical_key {
					if event.state.is_pressed() {
						event_loop.exit();
					}
				}
			}
			WindowEvent::RedrawRequested => {
				let now = Instant::now();
				let elaspsed = now.duration_since(window.context.last_render);
				window.context.last_render = now;
				window.context.update(elaspsed.as_secs_f32());

				let frame = match window.surface.get_current_texture() {
					Ok(frame) => frame,
					Err(e) => {
						eprintln!("dropped frame: {e:?}");
						return;
					}
				};

				let view = frame.texture.create_view(&wgpu::TextureViewDescriptor::default());
				let mut encoder: CommandEncoder = window.device.create_command_encoder(&wgpu::CommandEncoderDescriptor {
					label: None,
				});
				window.context.render(&view, &window.device, &mut encoder);
				window.queue.submit(Some(encoder.finish()));
				frame.present();
			}
			_ => (),
		}
	}

	fn about_to_wait(&mut self, _event_loop: &ActiveEventLoop) {
		let window = self.window.as_mut().unwrap();
		window.window.request_redraw();
	}
}

fn main() {
	akizuki_cli::init_logging(LevelFilter::Debug);

	let event_loop = EventLoop::new().unwrap();
	event_loop.set_control_flow(ControlFlow::Poll);
	event_loop.run_app(&mut App::default()).unwrap();
}
