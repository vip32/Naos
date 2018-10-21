namespace Naos.Core.Common
{
    using System.Text.RegularExpressions;

    public static partial class Extensions
    {
        /// <summary>
        /// Compares strings with usage of wildcard *
        /// </summary>
        /// <param name="source">the source string</param>
        /// <param name="value">the value string to compare to</param>
        /// <param name="ignoreCase">Ignore case</param>
        /// <returns>true if equal, otherwhise false</returns>
        public static bool EqualsWildcard(
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
    }
}
