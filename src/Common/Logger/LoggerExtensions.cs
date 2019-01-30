namespace Naos.Core.Common
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public static class LoggerExtensions
    {
        public static ILogger LogJournal(this ILogger source, string type, string message, LogLevel level = LogLevel.Information, params object[] args)
        {
            if (!message.IsNullOrEmpty())
            {
                type = type ?? LogEventPropertyKeys.TrackMisc;
                using (source.BeginScope(new Dictionary<string, object>
                {
                    [LogEventPropertyKeys.TrackType] = LogEventTrackTypeValues.Journal,
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
