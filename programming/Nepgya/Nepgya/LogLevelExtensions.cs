// 
// This file is licensed under the terms of the Simple Non Code License (SNCL) 2.3.0.
// The full license text can be found in the file named LICENSE.txt.
// Written originally by Alexandre Quoniou in 2018.
//

using System.Collections.Generic;
using System.Collections.Immutable;
using DSharpPlus;
using Serilog.Events;

namespace Nepgya
{
    static class LogLevelExtensions
    {
        private static readonly ImmutableDictionary<LogLevel, LogEventLevel> LogLevelMap = new Dictionary<LogLevel, LogEventLevel>()
            {
                [LogLevel.Critical] = LogEventLevel.Fatal,
                [LogLevel.Error] = LogEventLevel.Error,
                [LogLevel.Warning] = LogEventLevel.Warning,
                [LogLevel.Info] = LogEventLevel.Information,
                [LogLevel.Debug] = LogEventLevel.Debug,
            }
            .ToImmutableDictionary();

        public static LogEventLevel ToLogEventLevel(this LogLevel logLevel)
        {
            return LogLevelMap.TryGetValue(logLevel, out var logEventLevel) ? logEventLevel : LogEventLevel.Verbose;
        }
    }
}