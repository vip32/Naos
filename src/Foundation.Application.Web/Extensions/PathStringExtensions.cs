namespace Naos.Foundation.Application
{
    using System.Diagnostics;
    using Microsoft.AspNetCore.Http;

    public static partial class PathStringExtensions
    {
        /// <summary>
        /// Safeky compares the source to the value string.
        /// </summary>
        /// <param name="source">the source string.</param>
        /// <param name="value">the value string to compare to.</param>
        /// <param name="comparisonType">the comparison type.</param>
        /// <returns>true if equal, otherwhise false.</returns>
        [DebuggerStepThrough]
        public static bool SafeEquals(
            this PathString source,
            string value,
            System.StringComparison comparisonType = System.StringComparison.OrdinalIgnoreCase)
        {
            if (source == default && value == null)
            {
                return true;
            }

            if (source == default && value != null)
            {
                return false;
            }

            return source.Equals(new PathString(value), comparisonType);
        }
    }
}
