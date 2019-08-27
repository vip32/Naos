namespace Naos.Foundation
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Extensions.Primitives;

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
        public static T To<T>(this object source, bool throws = false, T defaultValue = default, CultureInfo cultureInfo = null)
        {
            if (source == null)
            {
                return defaultValue;
            }

            var toType = typeof(T);

            try
            {
                if (source.GetType() == typeof(StringValues))
                {
                    return source.ToString().To<T>();
                }

                if (toType == typeof(Guid))
                {
                    return (T)TypeDescriptor.GetConverter(toType).ConvertFrom(Convert.ToString(source, cultureInfo ?? CultureInfo.InvariantCulture));
                }

                if (toType is IConvertible || (toType.IsValueType && !toType.IsEnum))
                {
                    return (T)Convert.ChangeType(source, toType, cultureInfo ?? CultureInfo.InvariantCulture);
                }

                if (toType.IsEnum && (source is string || source is int || source is decimal || source is double || source is float))
                {
                    try
                    {
                        return (T)Enum.Parse(toType, source.ToString());
                    }
                    catch (ArgumentException)
                    {
                        return default;
                    }
                }

                return (T)source;
            }
            catch (FormatException)
            {
                if (throws)
                {
                    throw;
                }

                return defaultValue;
            }
            catch (InvalidCastException)
            {
                if (throws)
                {
                    throw;
                }

                return defaultValue;
            }
        }

        /// <summary>
        /// Converts an object to a value type using <see cref="Convert.ChangeType(object,TypeCode)" />.</summary>
        /// <typeparam name="T">The target object type.</typeparam>
        /// <param name="source">The object to be converted.</param>
        /// <param name="toType">The type to be converted to.</param>
        /// <param name="throws">if set to <c>true</c> throws exceptions when conversion fails.</param>
        /// <param name="defaultValue">The default value to return when conversion fails.</param>
        /// <returns>
        /// Converted object.
        /// </returns>
        [DebuggerStepThrough]
        public static object To(this object source, Type toType, bool throws = false, object defaultValue = default, CultureInfo cultureInfo = null)
        {
            if (source == null)
            {
                if (toType == typeof(Guid))
                {
                    return Guid.Empty;
                }

                return defaultValue;
            }

            try
            {
                if (source.GetType() == typeof(StringValues))
                {
                    return source.ToString().To(toType);
                }

                if (toType == typeof(Guid))
                {
                    return TypeDescriptor.GetConverter(toType).ConvertFrom(Convert.ToString(source, cultureInfo ?? CultureInfo.InvariantCulture));
                }

                if (toType is IConvertible || (toType.IsValueType && !toType.IsEnum))
                {
                    return Convert.ChangeType(source, toType, cultureInfo ?? CultureInfo.InvariantCulture);
                }

                if (toType.IsEnum && (source is string || source is int || source is decimal || source is double || source is float))
                {
                    try
                    {
                        return Enum.Parse(toType, source.ToString());
                    }
                    catch (ArgumentException)
                    {
                        return default;
                    }
                }

                return source;
            }
            catch (FormatException)
            {
                if (throws)
                {
                    throw;
                }

                return defaultValue;
            }
            catch (InvalidCastException)
            {
                if (throws)
                {
                    throw;
                }

                return defaultValue;
            }
        }

        [DebuggerStepThrough]
        public static bool TryTo<T>(this object source, out T result, CultureInfo cultureInfo = null)
        {
            if (source == null)
            {
                result = default;
                return false;
            }

            var toType = typeof(T);

            try
            {
                if (toType == typeof(Guid))
                {
                    result = (T)TypeDescriptor.GetConverter(toType).ConvertFrom(Convert.ToString(source, cultureInfo ?? CultureInfo.InvariantCulture));
                    return true;
                }

                if (toType is IConvertible || (toType.IsValueType && !toType.IsEnum))
                {
                    result = (T)Convert.ChangeType(source, toType, cultureInfo ?? CultureInfo.InvariantCulture);
                    return true;
                }

                if (toType.IsEnum && (source is string || source is int || source is decimal || source is double || source is float))
                {
                    result = (T)Enum.Parse(toType, source.ToString());
                    return true;
                }

                result = (T)source;
                return true;
            }
            catch (OverflowException)
            {
                result = default;
                return false;
            }
            catch (FormatException)
            {
                result = default;
                return false;
            }
            catch (InvalidCastException)
            {
                result = default;
                return false;
            }
        }

        [DebuggerStepThrough]
        public static bool TryTo(this object source, Type toType, out object result, CultureInfo cultureInfo = null)
        {
            if (source == null)
            {
                result = default;
                return false;
            }

            try
            {
                if (toType == typeof(Guid))
                {
                    result = TypeDescriptor.GetConverter(toType).ConvertFrom(Convert.ToString(source, cultureInfo ?? CultureInfo.InvariantCulture));
                    return true;
                }

                if (toType is IConvertible || (toType.IsValueType && !toType.IsEnum))
                {
                    result = Convert.ChangeType(source, toType, cultureInfo ?? CultureInfo.InvariantCulture);
                    return true;
                }

                if (toType.IsEnum && source is string)
                {
                    result = Enum.Parse(toType, source.ToString());
                    return true;
                }

                if (toType.IsEnum && source is int)
                {
                    result = Enum.ToObject(toType, (int)source);
                    return true;
                }

                result = source;
                return true;
            }
            catch (OverflowException)
            {
                result = default;
                return false;
            }
            catch (FormatException)
            {
                result = default;
                return false;
            }
            catch (InvalidCastException)
            {
                result = default;
                return false;
            }
        }

        private static TEnum ToEnum<TEnum>(this int source)
            where TEnum : struct//, IComparable, IFormattable, IConvertible
        {
            if (!typeof(TEnum).IsEnum)
            {
                return default;
            }

            if (Enum.IsDefined(typeof(TEnum), source))
            {
                //if a straightforward single value, return that
                return (TEnum)Enum.ToObject(typeof(TEnum), source);
            }

            var values = Enum.GetValues(typeof(TEnum))
                .Cast<int>()
                .ToList();

            var isBitwise = values.Select((n, i) =>
            {
                if (i < 2)
                {
                    return n == 0 || n == 1;
                }

                return n / 2 == values[i - 1];
            })
            .All(y => y);

            var maxValue = values.Sum();

            if (Enum.TryParse(source.ToString(), out TEnum result)
                && (source <= maxValue || !isBitwise))
            {
                //if it can be parsed as a bitwise enum with multiple flags,
                //or is not bitwise, return the result of TryParse
                return result;
            }

            //If the value is higher than all possible combinations,
            //remove the high imaginary values not accounted for in the enum
            var excess = Enumerable
                .Range(0, 32)
                .Select(n => (int)Math.Pow(2, n))
                .Where(n => n <= source && n > 0 && !values.Contains(n))
                .Sum();

            return Enum.TryParse((source - excess).ToString(), out result) ? result : default;
        }
    }
}