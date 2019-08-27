namespace Naos.Foundation
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    public static class TimeSpanExtensions
    {
        [DebuggerStepThrough]
        public static CancellationTokenSource ToCancellationTokenSource(this TimeSpan source)
        {
            if (source == TimeSpan.Zero)
            {
                var result = new CancellationTokenSource();
                result.Cancel();
                return result;
            }

            if (source.Ticks > 0)
            {
                return new CancellationTokenSource(source);
            }

            return new CancellationTokenSource();
        }

        [DebuggerStepThrough]
        public static CancellationTokenSource ToCancellationTokenSource(this TimeSpan? source)
        {
            if (source.HasValue)
            {
                return source.Value.ToCancellationTokenSource();
            }

            return new CancellationTokenSource();
        }

        [DebuggerStepThrough]
        public static CancellationTokenSource ToCancellationTokenSource(this TimeSpan? source, TimeSpan defaultTimeout)
        {
            return (source ?? defaultTimeout).ToCancellationTokenSource();
        }

        [DebuggerStepThrough]
        public static TimeSpan Min(this TimeSpan source, TimeSpan other)
        {
            return source.Ticks > other.Ticks ? other : source;
        }

        [DebuggerStepThrough]
        public static TimeSpan Max(this TimeSpan source, TimeSpan other)
        {
            return source.Ticks < other.Ticks ? other : source;
        }

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Ticks</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Ticks(this long number) => TimeSpan.FromTicks(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Milliseconds</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Milliseconds(this long number) => TimeSpan.FromMilliseconds(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Seconds</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Seconds(this long number) => TimeSpan.FromSeconds(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Minutes</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Minutes(this long number) => TimeSpan.FromMinutes(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Hours</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Hours(this long number) => TimeSpan.FromHours(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Days</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Days(this long number) => TimeSpan.FromDays(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Weeks</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Weeks(this long number) => TimeSpan.FromDays(number * 7);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Ticks</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Ticks(this int number) => TimeSpan.FromTicks(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Milliseconds</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Milliseconds(this int number) => TimeSpan.FromMilliseconds(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Seconds</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Seconds(this int number) => TimeSpan.FromSeconds(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Minutes</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Minutes(this int number) => TimeSpan.FromMinutes(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Hours</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Hours(this int number) => TimeSpan.FromHours(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Days</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Days(this int number) => TimeSpan.FromDays(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Weeks</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Weeks(this int number) => TimeSpan.FromDays(number * 7);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Ticks</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Ticks(this short number) => TimeSpan.FromTicks(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Milliseconds</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Milliseconds(this short number) => TimeSpan.FromMilliseconds(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Seconds</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Seconds(this short number) => TimeSpan.FromSeconds(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Minutes</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Minutes(this short number) => TimeSpan.FromMinutes(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Hours</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Hours(this short number) => TimeSpan.FromHours(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Days</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Days(this short number) => TimeSpan.FromDays(number);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> represented by <paramref name="number"/> as <c>Weeks</c>.
        /// </summary>
        [DebuggerStepThrough]
        public static TimeSpan Weeks(this short number) => TimeSpan.FromDays(number * 7);
    }
}
