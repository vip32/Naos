namespace Naos.Core.Common
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public static class LoggerExtensions
    {
        public static ILogger LogJournal(
            this ILogger source,
            string type,
            string message,
            string key = null,
            string name = null,
            LogLevel level = LogLevel.Information,
            TimeSpan? duration = null,
            params object[] args)
        {
            if(!message.IsNullOrEmpty())
            {
                type ??= LogEventPropertyKeys.TrackMisc;
                duration ??= TimeSpan.Zero;
                using(source.BeginScope(new Dictionary<string, object>
                {
                    [LogEventPropertyKeys.TrackType] = LogEventTrackTypeValues.Journal,
                    [LogEventPropertyKeys.TrackDuration] = duration.Value.Milliseconds,
                    [LogEventPropertyKeys.TrackKey] = key,
                    [LogEventPropertyKeys.TrackName] = name,
                    [type] = true
                }))
                {
                    source.Log(level, message, args);
                }
            }

            return source;
        }
    }
}
