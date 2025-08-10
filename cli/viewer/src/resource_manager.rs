use akizuki::error::{AkizukiError, AkizukiResult};
use akizuki::identifiers::ResourceId;
use akizuki::manager::ResourceManager;
use akizuki::table::BigWorldTableRecord;
use bevy::app::{App, Plugin};
use bevy::prelude::Resource;

#[derive(Resource)]
pub struct AkizukiResourceManager {
	pub manager: ResourceManager,
	pub validate: bool,
}

impl AkizukiResourceManager {
	pub fn load_asset(&self, resource_id: ResourceId) -> AkizukiResult<Vec<u8>> {
		self.manager.load_asset(resource_id, self.validate)
	}

	pub fn load_asset_by_path(&self, path: String) -> AkizukiResult<Vec<u8>> {
		self.load_asset(ResourceId::new(path.as_str()))
	}

	pub fn load_resource(&self, resource_id: ResourceId) -> AkizukiResult<&BigWorldTableRecord> {
		let db = self.manager.big_world_database.as_ref().unwrap();
		db.open(resource_id)
	}

	pub fn load_resource_by_path(&self, path: String) -> AkizukiResult<&BigWorldTableRecord> {
		self.load_resource(ResourceId::new(path.as_str()))
	}
}

pub struct AkizukiResourcePlugin {
	pub install_path: String,
	pub install_version: Option<i64>,
	pub validate: bool,
}

// avoid the entire asset server system.
impl Plugin for AkizukiResourcePlugin {
	fn build(&self, app: &mut App) {
		let install_path = self.install_path.clone();
		let install_version = self.install_version.clone();
		let validate = self.validate;

		let mut manager = ResourceManager::new(&install_path, install_version, validate).expect("could not create manager");

		if let Err(err) = manager.load_asset_database(validate) {
			panic!("load assets db: {:?}", err);
		}

		app.insert_resource(AkizukiResourceManager {
			manager,
			validate,
		});
	}
}
