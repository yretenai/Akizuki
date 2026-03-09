// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Akizuki.Moo;
using DragonLib;

namespace Akizuki.Tests;

public class StructTests {
	[SetUp]
	public void Setup() {
		Helpers.ResetCulture();
	}

	[Test]
	public void BigWorldHeaderTestPFSI() {
		ReadOnlySpan<byte> sample = Convert.FromHexString("49534650 00000002 01020304 40000000".Replace(" ", string.Empty, StringComparison.Ordinal));
		var header = MemoryMarshal.Read<BigWorldHeader>(sample);

		Assert.Multiple(() => {
			Assert.That(header.Magic, Is.EqualTo(BigWorldMagic.PackageIndex));
			Assert.That(header.Version, Is.EqualTo(new BigWorldVersion(2)));
			Assert.That(header.FileChecksum, Is.EqualTo(0x4030201));
			Assert.That(header.PointerSize, Is.EqualTo(64));
		});
	}

	[Test]
	public void BigWorldHeaderTestBWDB() {
		ReadOnlySpan<byte> sample = Convert.FromHexString("42445742 00000101 01020304 40000000".Replace(" ", string.Empty, StringComparison.Ordinal));
		var header = MemoryMarshal.Read<BigWorldHeader>(sample);

		Assert.Multiple(() => {
			Assert.That(header.Magic, Is.EqualTo(BigWorldMagic.AssetDatabase));
			Assert.That(header.Version, Is.EqualTo(new BigWorldVersion(1, 1)));
			Assert.That(header.FileChecksum, Is.EqualTo(0x4030201));
			Assert.That(header.PointerSize, Is.EqualTo(64));
		});
	}
}
