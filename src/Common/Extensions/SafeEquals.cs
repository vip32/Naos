namespace Naos.Core.Common
{
    public static partial class Extensions
    {
        /// <summary>
        /// Safeky compares the source to the value string
        /// </summary>
        /// <param name="source">the source string</param>
        /// <param name="value">the value string to compare to</param>
        /// <param name="comparisonType">the comparison type</param>
        /// <returns>true if equal, otherwhise false</returns>
        public static bool SafeEquals(
            this string source,
            string value,
            System.StringComparison comparisonType = System.StringComparison.OrdinalIgnoreCase)
        {
            if(source == null && value == null)
            {
                return true;
            }

            if(source == null && value != null)
            {
                return false;
            }

            return source.Equals(value, comparisonType);
        }
    }
}