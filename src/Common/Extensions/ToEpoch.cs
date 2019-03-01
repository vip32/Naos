namespace Naos.Core.Common
{
    using System;

    public static partial class Extensions
    {
        /// <summary>
        /// Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.
        /// </summary>
        /// <param name="source">The datetime source to convert.</param>
        /// <remarks>
        ///  Unix time represents the number of seconds that have elapsed since 1970-01-01T00:00:00Z (January 1, 1970, at 12:00 AM UTC).
        ///  It does not take leap seconds into account.
        ///  </remarks>
        /// <returns>
        /// The Unix timestamp
        /// </returns>
        public static long ToEpoch(this DateTime source)
        {
            // https://msdn.microsoft.com/en-us/library/system.datetimeoffset.tounixtimeseconds(v=vs.110).aspx
            return new DateTimeOffset(
                source.Year,
                source.Month,
                source.Day,
                source.Hour,
                source.Minute,
                source.Second,
                TimeSpan.Zero).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.
        /// </summary>
        /// <param name="source">The datetime source to convert.</param>
        /// <remarks>
        ///  Unix time represents the number of seconds that have elapsed since 1970-01-01T00:00:00Z (January 1, 1970, at 12:00 AM UTC).
        ///  It does not take leap seconds into account.
        ///  </remarks>
        /// <returns>
        /// The Unix timestamp
        /// </returns>
        public static long ToEpoch(this DateTimeOffset source)
        {
            // https://msdn.microsoft.com/en-us/library/system.datetimeoffset.tounixtimeseconds(v=vs.110).aspx
            return new DateTimeOffset(
                source.Year,
                source.Month,
                source.Day,
                source.Hour,
                source.Minute,
                source.Second,
                TimeSpan.Zero).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.
        /// </summary>
        /// <param name="source">The datetime source to convert.</param>
        /// <remarks>
        ///  Unix time represents the number of seconds that have elapsed since 1970-01-01T00:00:00Z (January 1, 1970, at 12:00 AM UTC).
        ///  It does not take leap seconds into account.
        ///  </remarks>
        /// <returns>
        /// The Unix timestamp
        /// </returns>
        public static long? ToEpoch(this DateTime? source)
        {
            if(!source.HasValue)
            {
                return null;
            }

            return ToEpoch(source.Value);
        }

        /// <summary>
        /// Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.
        /// </summary>
        /// <param name="source">The datetime source to convert.</param>
        /// <remarks>
        ///  Unix time represents the number of seconds that have elapsed since 1970-01-01T00:00:00Z (January 1, 1970, at 12:00 AM UTC).
        ///  It does not take leap seconds into account.
        ///  </remarks>
        /// <returns>
        /// The Unix timestamp
        /// </returns>
        public static long? ToEpoch(this DateTimeOffset? source)
        {
            if (!source.HasValue)
            {
                return null;
            }

            return ToEpoch(source.Value);
        }
    }
}
