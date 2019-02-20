namespace Naos.Core.Common
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public static partial class Extensions
    {
        /// <summary>
        /// Compares strings with usage of pattern *
        /// </summary>
        /// <param name="source">the source string</param>
        /// <param name="value">the value string to compare to</param>
        /// <param name="ignoreCase">Ignore case</param>
        /// <returns>true if equal, otherwhise false</returns>
        public static bool EqualsPattern(
            this string source,
            string value,
            bool ignoreCase = true)
        {
            if (source == null && value == null)
            {
                return true;
            }

            if (source == null)
            {
                return false;
            }

            var regexp = Regex.Escape(value).Replace("\\*", ".*");

            return Regex.IsMatch(source, "^" + (ignoreCase ? "(?i)" : string.Empty) + regexp + "$");
        }

        /// <summary>
        /// Compares strings with usage of pattern *
        /// </summary>
        /// <param name="source">the source string</param>
        /// <param name="values">the value strings to compare to</param>
        /// <param name="ignoreCase">Ignore case</param>
        /// <returns>true if equal, otherwhise false</returns>
        public static bool EqualsPatternAny(
            this string source,
            IEnumerable<string> values,
            bool ignoreCase = true)
        {
            if (source == null && values == null)
            {
                return true;
            }

            if (values.IsNullOrEmpty())
            {
                return false;
            }

            foreach (var value in values.Safe())
            {
                if (value == null)
                {
                    continue;
                }

                if (source.EqualsPattern(value, ignoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
