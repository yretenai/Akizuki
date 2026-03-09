// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Moo;

namespace Akizuki.PackageFileSystem;

public class Package : BigWorldFile {
	public Package(Stream stream, bool validateChecksum = false) : base(stream, validateChecksum) { }
}
