// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using Akizuki.Structs.Data.Camouflage;
using DragonLib.IO;

namespace Akizuki.Data;

public static class CamouflageData {
	public static CamouflageRoot Create(IMemoryBuffer<byte> buffer, bool leaveOpen = false) {
		using var deferred = new DeferredDisposable(() => {
			if (!leaveOpen) {
				buffer.Dispose();
			}
		});

		var serializer = new XmlSerializer(typeof(CamouflageRoot));
		var textureSerializers = new Dictionary<string, XmlSerializer>();
		serializer.UnknownElement += (_, args) => {
			if (args.ObjectBeingDeserialized is Camouflage camouflage) {
				switch (args.Element.Name) {
					case "UV": {
						foreach (var element in args.Element.OfType<XmlElement>()) {
							camouflage.UV[element.Name] = CamouflageRoot.ConvertVec2(element.InnerText);
						}

						return;
					}
					// why can't xml deserialize True as true???
					case "useColorScheme": {
						if (bool.TryParse(args.Element.InnerText, out var value)) {
							camouflage.UseColorScheme = value;
						} else {
							throw new InvalidOperationException();
						}

						return;
					}
					case "Shaders": {
						camouflage.Shaders ??= [];
						foreach (var element in args.Element.OfType<XmlElement>()) {
							camouflage.Shaders[element.Name] = element.InnerText;
						}

						return;
					}
					case "Textures": {
						foreach (var element in args.Element.OfType<XmlElement>()) {
							using var reader = new XmlNodeReader(element);
							if (!textureSerializers.TryGetValue(element.Name, out var textureSerializer)) {
								textureSerializer = textureSerializers[element.Name] = new XmlSerializer(typeof(CamouflageTexture), new XmlRootAttribute(element.Name));
							}

							camouflage.Textures[element.Name] = (CamouflageTexture) textureSerializer.Deserialize(reader)!;
						}

						return;
					}
				}
			}

			if (Debugger.IsAttached) {
				Debugger.Break();
			}
		};
		using var pin = buffer.Memory.Pin();
		unsafe {
			using var stream = new UnmanagedMemoryStream((byte*) pin.Pointer, buffer.Memory.Length);
			var obj = serializer.Deserialize(stream);
			return (CamouflageRoot) obj!;
		}
	}
}
