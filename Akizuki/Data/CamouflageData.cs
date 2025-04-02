// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Xml.Linq;
using Akizuki.Structs.Data.Camouflage;
using DragonLib.IO;

namespace Akizuki.Data;

public class CamouflageData {
	public CamouflageData(IMemoryBuffer<byte> buffer, bool leaveOpen = false) {
		using var deferred = new DeferredDisposable(() => {
			if (!leaveOpen) {
				buffer.Dispose();
			}
		});


		using var pin = buffer.Memory.Pin();
		unsafe {
			using var stream = new UnmanagedMemoryStream((byte*) pin.Pointer, buffer.Memory.Length);
			var element = XDocument.Load(stream);

			if (element.Root == null) {
				return;
			}


			if (element.Root.Element("ShipGroups") is { } shipGroups) {
				foreach (var shipGroup in shipGroups.Elements()) {
					ShipGroups.Add(new CamouflageShipGroup(shipGroup));
				}
			}

			if (element.Root.Element("ColorSchemes") is { } colorSchemes) {
				foreach (var colorScheme in colorSchemes.Elements()) {
					ColorSchemes.Add(new CamouflageColorScheme(colorScheme));
				}
			}

			if (element.Root.Element("Camouflages") is { } camouflages) {
				foreach (var camouflage in camouflages.Elements()) {
					Camouflages.Add(new Camouflage(camouflage));
				}
			}
		}
	}

	public List<CamouflageShipGroup> ShipGroups { get; set; } = [];

	public List<CamouflageColorScheme> ColorSchemes { get; set; } = [];

	public List<Camouflage> Camouflages { get; set; } = [];
}
