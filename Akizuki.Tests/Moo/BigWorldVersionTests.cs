// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Akizuki.Moo;
using DragonLib;

namespace Akizuki.Tests.Moo;

public class BigWorldVersionTests {
	[SetUp]
	public void Setup() {
		Helpers.ResetCulture();
	}

	[Test]
	public void CompareGreaterThanTest() {
		var a = new BigWorldVersion(1, 2, 3, 4);
		var b = new BigWorldVersion(4, 3, 2, 1);

		Assert.Multiple(() => {
			Assert.That(b, Is.GreaterThan(a));
			Assert.That(b > a);
		});
	}

	[Test]
	public void CompareLessThanTest() {
		var a = new BigWorldVersion(1, 2, 3, 4);
		var b = new BigWorldVersion(4, 3, 2, 1);

		Assert.Multiple(() => {
			Assert.That(a, Is.LessThan(b));
			Assert.That(a < b);
		});
	}

	[Test]
	public void CompareGreaterThanOrEqualTest() {
		var a = new BigWorldVersion(1, 2, 3, 4);
		var b = new BigWorldVersion(4, 3, 2, 1);
		var c = new BigWorldVersion(4, 3, 2, 1);

		Assert.Multiple(() => {
			Assert.That(b, Is.GreaterThanOrEqualTo(a));
			Assert.That(b, Is.GreaterThanOrEqualTo(c));
			Assert.That(b >= a);
			Assert.That(b >= c);
		});
	}

	[Test]
	public void CompareLessThanOrEqualTest() {
		var a = new BigWorldVersion(1, 2, 3, 4);
		var b = new BigWorldVersion(4, 3, 2, 1);
		var c = new BigWorldVersion(1, 2, 3, 4);

		Assert.Multiple(() => {
			Assert.That(a, Is.LessThanOrEqualTo(b));
			Assert.That(a, Is.LessThanOrEqualTo(c));
			Assert.That(a <= b);
			Assert.That(a <= c);
		});
	}

	[Test]
	public void CompareEqualTest() {
		var a = new BigWorldVersion(1, 2, 3, 4);
		var b = new BigWorldVersion(1, 2, 3, 4);

		Assert.Multiple(() => {
			Assert.That(a, Is.EqualTo(b));
			Assert.That(a == b);
		});
	}

	[Test]
	public void CompareNotEqualTest() {
		var a = new BigWorldVersion(1, 2, 3, 4);
		var b = new BigWorldVersion(4, 3, 2, 1);

		Assert.Multiple(() => {
			Assert.That(a, Is.Not.EqualTo(b));
			Assert.That(a != b);
		});
	}

	[Test]
	public void TestByteOrderRead() {
		ReadOnlySpan<byte> bytes = [0x1, 0x2, 0x3, 0x4];
		var a = new BigWorldVersion(bytes[3], bytes[2], bytes[1], bytes[0]);
		var b = MemoryMarshal.Read<BigWorldVersion>(bytes);
		Assert.That(a, Is.EqualTo(b));
	}

	[Test]
	public void TestByteOrderWrite() {
		Span<byte> bytes = [0, 0, 0, 0];
		ReadOnlySpan<byte> expected = [0x1, 0x2, 0x3, 0x4];
		var a = new BigWorldVersion(4, 3, 2, 1);
		MemoryMarshal.Write(bytes, in a);
		Assert.That(expected.SequenceEqual(bytes));
	}
}
