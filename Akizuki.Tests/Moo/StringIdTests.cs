// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Moo;
using Pluto;

namespace Akizuki.Tests.Moo;

public class StringIdTests {
	[SetUp]
	public void Setup() {
		Helpers.ResetCulture();
	}

	private const string TEST_STRING = "Akizuki";

	[Test]
	public void HashTest() {
		var str = new StringId(TEST_STRING);
		Assert.That(str.Hash, Is.EqualTo(0x8d949450u));
	}

	[Test]
	public void AlignedHashTest() {
		var str = new StringId("Akizuki_");
		Assert.That(str.Hash, Is.EqualTo(0xe344aed1u));
	}

	[Test]
	public void StringTest() {
		var str = new StringId(TEST_STRING);
		StringId.Lookup.Clear();
		StringId.Lookup[0x8d949450] = TEST_STRING;
		Assert.That(str.ToString(), Is.EqualTo(TEST_STRING));
	}

	[Test]
	public void StringUnknownTest() {
		var str = new StringId(TEST_STRING);
		StringId.Lookup.Clear();
		Assert.That(str.ToString(), Is.EqualTo("0x8d949450"));
	}

	[Test]
	public void DebugStringTest() {
		var str = new StringId(TEST_STRING);
		StringId.Lookup.Clear();
		StringId.Lookup[0x8d949450] = TEST_STRING;
		Assert.That(str.ToDebugString(), Is.EqualTo($"\"{TEST_STRING}\" (0x8d949450)"));
	}
}
