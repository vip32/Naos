namespace Naos.Foundation
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;

    public static partial class Extensions
    {
        /// <summary>
        /// Converts an object to a value type using <see cref="Convert.ChangeType(object,TypeCode)" />.</summary>
        /// <typeparam name="T">The target object type.</typeparam>
        /// <param name="source">The object to be converted.</param>
        /// <param name="throws">if set to <c>true</c> throws exceptions when conversion fails.</param>
        /// <param name="defaultValue">The default value to return when conversion fails.</param>
        /// <returns>
        /// Converted object.
        /// </returns>
        [DebuggerStepThrough]
        public static T To<T>(this object source, bool throws = false, T defaultValue = default)
            where T : struct
        {
            if(source == null)
            {
                return default;
            }

            try
            {
                if(typeof(T) == typeof(Guid))
                {
                    return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(source.ToString());
                }

                return (T)Convert.ChangeType(source, typeof(T), CultureInfo.InvariantCulture);
            }
            catch(FormatException)
            {
                if(throws)
                {
                    throw;
                }

                return defaultValue;
            }
        }

        [DebuggerStepThrough]
        public static bool TryTo<T>(this object source, out T result)
            where T : struct
        {
            if(source == null)
            {
                result = default;
                return false;
            }

            try
            {
                if(typeof(T) == typeof(Guid))
                {
                    result = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(source.ToString());
                    return true;
                }

                result = (T)Convert.ChangeType(source, typeof(T), CultureInfo.InvariantCulture);
                return true;
            }
            catch(OverflowException)
            {
                result = default;
                return false;
            }
            catch(FormatException)
            {
                result = default;
                return false;
            }
        }
    }
}