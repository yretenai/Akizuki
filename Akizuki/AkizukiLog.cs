// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Core;

namespace Akizuki;

public static class AkizukiLog {
	public static ILogger Logger { get; set; } = Log.Logger;

	[Conditional("DEBUG")] [MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Debug(string messageTemplate) => Logger.Debug(messageTemplate);

	[Conditional("DEBUG")] [MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Debug(string messageTemplate, params object?[]? propertyValues) => Logger.Debug(messageTemplate, propertyValues);

	[Conditional("DEBUG")] [MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Debug(Exception ex, string messageTemplate) => Logger.Debug(ex, messageTemplate);

	[Conditional("DEBUG")] [MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Debug(Exception ex, string messageTemplate, params object?[]? propertyValues) => Logger.Debug(ex, messageTemplate, propertyValues);

	[Conditional("DEBUG")] [MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Verbose(string messageTemplate) => Logger.Verbose(messageTemplate);

	[Conditional("DEBUG")] [MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Verbose(string messageTemplate, params object?[]? propertyValues) => Logger.Verbose(messageTemplate, propertyValues);

	[Conditional("DEBUG")] [MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Verbose(Exception ex, string messageTemplate) => Logger.Verbose(ex, messageTemplate);

	[Conditional("DEBUG")] [MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Verbose(Exception ex, string messageTemplate, params object?[]? propertyValues) => Logger.Verbose(ex, messageTemplate, propertyValues);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Information(string messageTemplate) => Logger.Information(messageTemplate);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Information(string messageTemplate, params object?[]? propertyValues) => Logger.Information(messageTemplate, propertyValues);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Information(Exception ex, string messageTemplate) => Logger.Information(ex, messageTemplate);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Information(Exception ex, string messageTemplate, params object?[]? propertyValues) => Logger.Information(ex, messageTemplate, propertyValues);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Warning(string messageTemplate) => Logger.Warning(messageTemplate);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Warning(string messageTemplate, params object?[]? propertyValues) => Logger.Warning(messageTemplate, propertyValues);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Warning(Exception ex, string messageTemplate) => Logger.Warning(ex, messageTemplate);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Warning(Exception ex, string messageTemplate, params object?[]? propertyValues) => Logger.Warning(ex, messageTemplate, propertyValues);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Error(string messageTemplate) => Logger.Error(messageTemplate);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Error(string messageTemplate, params object?[]? propertyValues) => Logger.Error(messageTemplate, propertyValues);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Error(Exception ex, string messageTemplate) => Logger.Error(ex, messageTemplate);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Error(Exception ex, string messageTemplate, params object?[]? propertyValues) => Logger.Error(ex, messageTemplate, propertyValues);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Fatal(string messageTemplate) => Logger.Error(messageTemplate);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Fatal(string messageTemplate, params object?[]? propertyValues) => Logger.Fatal(messageTemplate, propertyValues);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Fatal(Exception ex, string messageTemplate) => Logger.Fatal(ex, messageTemplate);

	[MessageTemplateFormatMethod("messageTemplate")] [MethodImpl(MethodConstants.Inline)]
	public static void Fatal(Exception ex, string messageTemplate, params object?[]? propertyValues) => Logger.Fatal(ex, messageTemplate, propertyValues);
}
