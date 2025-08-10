// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

// https://bevy.org/examples/3d-rendering/3d-scene/

mod resource_manager;

use std::f32::consts::PI;

use akizuki::identifiers::ResourceId;
use bevy::pbr::CascadeShadowConfigBuilder;
use bevy::prelude::*;
use bevy_panorbit_camera::*;
use clap::Parser;

use crate::resource_manager::{AkizukiResourceManager, AkizukiResourcePlugin};

#[derive(Parser)]
#[command(version, about)]
struct Cli {
	#[arg(index = 1, required = true, help = "path to the game installation directory")]
	install_path: String,

	#[arg(index = 2, help = "version number of the game, if not set will try to find the latest version")]
	install_version: Option<i64>,

	#[arg(long, help = "validate the data that's being processed")]
	validate: bool,
}

#[derive(Debug, Default, Clone, Resource)]
struct AkizukiSceneNode {
	pub model: Option<ResourceId>,
	pub skin: Option<ResourceId>,
	pub position: Vec3,
	pub rotation: Quat,
	pub scale: Vec3,
	pub matrix: Option<Mat4>,
	pub child_nodes: Vec<AkizukiSceneNode>,
}

fn main() {
	akizuki::format::oodle::init();

	let args = Cli::parse();

	App::new()
		.add_plugins(DefaultPlugins)
		.add_plugins(AkizukiResourcePlugin {
			install_path: args.install_path,
			install_version: args.install_version,
			validate: args.validate,
		})
		.insert_resource(AkizukiSceneNode {
			model: None,
			skin: None,
			position: Default::default(),
			rotation: Default::default(),
			scale: Vec3::new(-1f32, 1f32, 1f32),
			matrix: None,
			child_nodes: vec![],
		})
		.add_plugins(PanOrbitCameraPlugin)
		.add_systems(Startup, setup)
		.run();
}

fn setup(
	mut commands: Commands,
	resources: Res<AkizukiResourceManager>,
	mut meshes: ResMut<Assets<Mesh>>,
	mut materials: ResMut<Assets<StandardMaterial>>,
) {
	commands.spawn((Name::new("Camera"), PanOrbitCamera::default(), Transform::from_xyz(5.0, 5.0, 5.0).looking_at(Vec3::ZERO, Vec3::Y)));

	commands.spawn((
		Name::new("Cube"),
		Mesh3d(meshes.add(Cuboid::default())),
		MeshMaterial3d(materials.add(Color::WHITE)),
		Transform::from_xyz(1.5, 0.51, 1.5),
	));

	commands.spawn((
		Name::new("Ambient"),
		AmbientLight {
			brightness: 0.05,
			..default()
		},
	));

	commands.spawn((
		Name::new("Light"),
		DirectionalLight {
			illuminance: light_consts::lux::OVERCAST_DAY,
			shadows_enabled: true,
			..default()
		},
		Transform {
			translation: Vec3::new(0.0, 2.0, 0.0),
			rotation: Quat::from_rotation_x(-PI / 4.),
			..default()
		},
		CascadeShadowConfigBuilder {
			first_cascade_far_bound: 2.0,
			maximum_distance: 40.0,
			..default()
		}
		.build(),
	));
}
