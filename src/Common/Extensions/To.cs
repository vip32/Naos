namespace Naos.Core.Common
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public static partial class Extensions
    {
        /// <summary>
        /// Converts an object to a value type using <see cref="Convert.ChangeType(object,System.TypeCode)"/>
        /// </summary>
        /// <param name="source">The object to be converted</param>
        /// <typeparam name="T">The target object type</typeparam>
        /// <returns>Converted object</returns>
        public static T To<T>(this object source)
            where T : struct
        {
            if (typeof(T) == typeof(Guid))
            {
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(source.ToString());
            }

            return (T)Convert.ChangeType(source, typeof(T), CultureInfo.InvariantCulture);
        }
    }
}