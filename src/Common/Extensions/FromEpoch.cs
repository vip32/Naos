namespace Naos.Core.Common
{
    using System;

    public static partial class Extensions
    {
        /// <summary>
        /// Creates a datetime for the provided epoch value.
        /// </summary>
        /// <param name="epoch">The epoch value.</param>
        /// <returns></returns>
        public static DateTime FromEpoch(long epoch)
        {
            return DateTimeOffset.FromUnixTimeSeconds(epoch).DateTime;
        }

        /// <summary>
        /// Creates a datetime for the provided epoch value.
        /// </summary>
        /// <param name="epoch">The epoch value.</param>
        /// <returns></returns>
        public static DateTime? FromEpoch(long? epoch)
        {
            if (!epoch.HasValue)
            {
                return null;
            }

            return DateTimeOffset.FromUnixTimeSeconds(epoch.Value).DateTime;
        }
    }
}
