namespace Naos.Core.Common
{
    using System;
    using System.Threading;

    public static class TimeSpanExtensions
    {
        public static CancellationTokenSource ToCancellationTokenSource(this TimeSpan source)
        {
            if(source == TimeSpan.Zero)
            {
                var result = new CancellationTokenSource();
                result.Cancel();
                return result;
            }

            if(source.Ticks > 0)
            {
                return new CancellationTokenSource(source);
            }

            return new CancellationTokenSource();
        }

        public static CancellationTokenSource ToCancellationTokenSource(this TimeSpan? source)
        {
            if(source.HasValue)
            {
                return source.Value.ToCancellationTokenSource();
            }

            return new CancellationTokenSource();
        }

        public static CancellationTokenSource ToCancellationTokenSource(this TimeSpan? source, TimeSpan defaultTimeout)
        {
            return (source ?? defaultTimeout).ToCancellationTokenSource();
        }

        public static TimeSpan Min(this TimeSpan source, TimeSpan other)
        {
            return source.Ticks > other.Ticks ? other : source;
        }

        public static TimeSpan Max(this TimeSpan source, TimeSpan other)
        {
            return source.Ticks < other.Ticks ? other : source;
        }
    }
}
