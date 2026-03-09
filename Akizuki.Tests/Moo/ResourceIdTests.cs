// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Akizuki.Moo;
using DragonLib;

namespace Akizuki.Tests.Moo;

public class ResourceIdTests {
	[SetUp]
	public void Setup() {
		Helpers.ResetCulture();
	}

	private const string TEST_STRING = "content/gameplay/japan/ship/destroyer/JSD011_Akizuki_1944/JSD011_Akizuki_1944.model";

	[Test]
	public void HashTest() {
		var str = new ResourceId(TEST_STRING);
		Assert.That(str.Hash, Is.EqualTo(0xdf5a921212a899eul));
	}

	[Test]
	public void AlignedHashTest() {
		var str = new ResourceId("content/gameplay/japan/ship/destroyer/JSD011_Akizuki_1944/JSD011_Akizuki_1944.mo");
		Assert.That(str.Hash, Is.EqualTo(0xa8fa81214165ecbul));
	}

	[Test]
	public void StringTest() {
		var str = new ResourceId(TEST_STRING);
		ResourceId.Lookup.Clear();
		ResourceId.Lookup[0x0df5a921212a899eul] = TEST_STRING;
		Assert.That(str.ToString(), Is.EqualTo(TEST_STRING));
	}

	[Test]
	public void StringUnknownTest() {
		var str = new ResourceId(TEST_STRING);
		ResourceId.Lookup.Clear();
		Assert.That(str.ToString(), Is.EqualTo("0xdf5a921212a899e"));
	}

	[Test]
	public void DebugStringTest() {
		var str = new ResourceId(TEST_STRING);
		ResourceId.Lookup.Clear();
		ResourceId.Lookup[0x0df5a921212a899eul] = TEST_STRING;
		Assert.That(str.ToDebugString(), Is.EqualTo($"\"{TEST_STRING}\" (0xdf5a921212a899e)"));
	}
}
