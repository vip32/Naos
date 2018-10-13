namespace Naos.Core.Common
{
    using System;

    public static partial class Extensions
    {
        /// <summary>
        /// Determines whether the specified string contains any of the items.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="items">The items.</param>
        /// <param name="comp">The comp.</param>
        /// <returns>
        ///   <c>true</c> if the specified items contains any; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsAny(
            this string source,
            string[] items,
            StringComparison comp = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            if (items == null)
            {
                return false;
            }

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                if (source.Contains(item, comp))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
