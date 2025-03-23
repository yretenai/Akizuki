// SPDX-FileCopyrightText: 2025 Ada N
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Akizuki.Structs.Data;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct PFSIndexHeader {
	public int FileNameCount { get; set; }
	public int FileInfoCount { get; set; }
	public int PackageCount { get; set; }
	public int Reserved { get; set; }
	public long FileNameSectionPtr { get; set; }
	public long FileInfoSectionPtr { get; set; }
	public long PackageSectionPtr { get; set; }
}
