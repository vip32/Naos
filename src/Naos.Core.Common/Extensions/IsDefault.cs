namespace Naos.Core.Common
{
    using System.Collections.Generic;

    public static partial class Extensions
    {
        /// <summary>
        /// Determines whether [is default].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is null or default] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDefault<T>(this T source)
        {
            return EqualityComparer<T>.Default.Equals(source, default);
        }
    }
}
