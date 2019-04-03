namespace Naos.Core.Common
{
    using System;

    public static partial class Extensions
    {
        /// <summary>
        /// Convert the string to a nullable bool.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="default">The default.</param>
        /// <returns></returns>
        public static bool? ToNullableBool(this string source, bool? @default = null)
        {
            if (source.IsNullOrEmpty())
            {
                return @default;
            }

            if (source.Equals("1", StringComparison.Ordinal))
            {
                return true;
            }

            if (source.Equals("0", StringComparison.Ordinal))
            {
                return false;
            }

            return !bool.TryParse(source, out var parsedValue) ? @default : parsedValue;
        }

        /// <summary>
        /// Convert the string to a bool.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="default">if set to <c>true</c> [default].</param>
        /// <returns></returns>
        public static bool ToBool(this string source, bool @default = false)
        {
            if (source.IsNullOrEmpty())
            {
                return @default;
            }

            if(source.Equals("1", StringComparison.Ordinal))
            {
                return true;
            }

            if (source.Equals("0", StringComparison.Ordinal))
            {
                return false;
            }

            return !bool.TryParse(source, out var parsedValue) ? @default : parsedValue;
        }
    }
}
