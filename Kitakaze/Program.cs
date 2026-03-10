// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using Akizuki;
using DragonLib;
using Serilog;
using Serilog.Events;

namespace Kitakaze;

public static class Program {
	public static void Main(string[] args) {
		Helpers.ResetCulture();
		Log.Logger = new LoggerConfiguration().MinimumLevel.Is(Debugger.IsAttached ? LogEventLevel.Debug : LogEventLevel.Information).WriteTo.Console().CreateLogger();
		using var manager = new ResourceManager(args[0]);
	}
}
