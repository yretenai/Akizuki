// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

mod scene;
mod shaders;

use std::sync::Arc;
use std::time::Instant;

use glam::Mat4;
use log::LevelFilter;
use pollster::block_on;
use wgpu::CommandEncoder;
use winit::application::ApplicationHandler;
use winit::dpi::{LogicalSize, PhysicalSize};
use winit::event_loop::{ActiveEventLoop, ControlFlow};
use winit::keyboard::{Key, NamedKey};
use winit::{event::WindowEvent, event_loop::EventLoop, window::Window};

use crate::scene::{Node, Scene};

struct AkizukiViewer {
	scene: Scene,
	time: f32,
	last_render: Instant,
}

struct AppWindow {
	device: wgpu::Device,
	queue: wgpu::Queue,
	window: Arc<Window>,
	surface_desc: wgpu::SurfaceConfiguration,
	surface: wgpu::Surface<'static>,
	context: AkizukiViewer,
}

#[derive(Default)]
struct App {
	window: Option<AppWindow>,
}

impl AkizukiViewer {
	fn new() -> Self {
		AkizukiViewer {
			scene: Scene::default(),
			time: 0.0,
			last_render: Instant::now(),
		}
	}

	fn update(&mut self, delta_time: f32) {
		self.time += delta_time;
	}

	fn render_nodes(&self, nodes: &Vec<Node>, matrix: Mat4, view: &wgpu::TextureView, device: &wgpu::Device, encoder: &mut CommandEncoder) {
		for node in nodes {
			let node_matrix = matrix * node.local_matrix;
			if let Some(mesh_key) = &node.mesh {
				{
					let mesh = &self.scene.meshes[mesh_key];
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
					rpass.set_index_buffer(mesh.index_buffer.slice(..), wgpu::IndexFormat::Uint16);
					rpass.set_vertex_buffer(0, mesh.index_buffer.slice(..));

					for submesh in &mesh.submeshes {
						let material = &submesh.material;
						let pipeline = &material.pipeline;

						let bind_group = device.create_bind_group(&wgpu::BindGroupDescriptor {
							layout: &material.bind_group_layout,
							entries: &[
								// todo: vert binding 0,
								// todo: frag bindnig 1,
							],
							label: None,
						});

						rpass.set_pipeline(&pipeline);
						rpass.set_bind_group(0, &bind_group, &[]);
						rpass.draw_indexed(submesh.first_index..submesh.last_index, submesh.first_vertex, 0..1);
					}
				}
			}

			self.render_nodes(&node.children, node_matrix, view, device, encoder);
		}
	}

	fn render(&mut self, view: &wgpu::TextureView, device: &wgpu::Device, encoder: &mut CommandEncoder) {
		self.render_nodes(&self.scene.nodes, Mat4::default(), view, device, encoder);
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

		let surface_desc = create_surface_desc(&size);

		surface.configure(&device, &surface_desc);

		let context = AkizukiViewer::new();

		Self {
			device,
			queue,
			window,
			surface_desc,
			surface,
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
