// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace Akizuki.Exceptions;

[Serializable]
public class CorruptDataException : Exception {
	public const string DefaultMessage = "Provided data is corrupt";

	public CorruptDataException() : base(DefaultMessage) { }
	public CorruptDataException(string message) : base(DefaultMessage + ": " + message) { }

	public CorruptDataException(string message, Exception? innerException) : base(DefaultMessage + ": " + message, innerException) { }
}
