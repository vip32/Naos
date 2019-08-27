namespace Naos.Foundation
{
    using System.Diagnostics;

    public static partial class Extensions
    {
        /// <summary>
        /// Removes the value string from the source string.
        /// </summary>
        /// <param name="source">the source to remove from.</param>
        /// <param name="value">the value to remove.</param>
        [DebuggerStepThrough]
        public static string Remove(this string source, string value)
        {
            if (source == null)
            {
                return source;
            }

            return source.Replace(value, string.Empty);
        }
    }
}
