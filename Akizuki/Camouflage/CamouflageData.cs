// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Xml.Linq;
using DragonLib.IO;
using DragonLib.IO.Binary;

namespace Akizuki.Camouflage;

public class CamouflageData {
	public CamouflageData(RentedArray<byte> buffer, bool leaveOpen = false) {
		using var deferred = new DeferredDisposable(() => {
			if (!leaveOpen) {
				buffer.Dispose();
			}
		});

		File.WriteAllBytes("camouflages.xml", buffer.Span);


		using var pin = buffer.Memory.Pin();
		unsafe {
			using var stream = new UnmanagedMemoryStream((byte*) pin.Pointer, buffer.Memory.Length);
			var element = XDocument.Load(stream);

			if (element.Root == null) {
				return;
			}

			if ((element.Root.Element("ShipGroups") ?? element.Root.Element("shipgroups.xml")) is { } shipGroups) {
				foreach (var shipGroup in shipGroups.Elements()) {
					ShipGroups.Add(new CamouflageShipGroup(shipGroup));
				}
			}

			if ((element.Root.Element("ColorSchemes") ?? element.Root.Element("colorschemes.xml")) is
				{ } colorSchemes) {
				foreach (var colorScheme in colorSchemes.Elements()) {
					ColorSchemes.Add(new CamouflageColorScheme(colorScheme));
				}
			}

			if ((element.Root.Element("Camouflages") ?? element.Root.Element("camouflages.xml")) is { } camouflages) {
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
