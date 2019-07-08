namespace Naos.Foundation
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

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
            where T : struct
        {
            if(source == null)
            {
                return defaultValue;
            }

            var toType = typeof(T);

            try
            {
                if(toType == typeof(Guid))
                {
                    return (T)TypeDescriptor.GetConverter(toType).ConvertFrom(Convert.ToString(source, cultureInfo ?? CultureInfo.InvariantCulture));
                }

                if(toType is IConvertible || (toType.IsValueType && !toType.IsEnum))
                {
                    return (T)Convert.ChangeType(source, toType, cultureInfo ?? CultureInfo.InvariantCulture);
                }

                if(toType.IsEnum && source is string)
                {
                    Enum.TryParse(source.ToString(), true, out T enumResult);
                    return enumResult;
                }

                return (T)source;
            }
            catch(FormatException)
            {
                if(throws)
                {
                    throw;
                }

                return defaultValue;
            }
            catch(InvalidCastException)
            {
                if(throws)
                {
                    throw;
                }

                return defaultValue;
            }
        }

        [DebuggerStepThrough]
        public static bool TryTo<T>(this object source, out T result, CultureInfo cultureInfo = null)
            where T : struct
        {
            if(source == null)
            {
                result = default;
                return false;
            }

            var toType = typeof(T);

            try
            {
                if(toType == typeof(Guid))
                {
                    result = (T)TypeDescriptor.GetConverter(toType).ConvertFrom(Convert.ToString(source, cultureInfo ?? CultureInfo.InvariantCulture));
                    return true;
                }

                if(toType is IConvertible || (toType.IsValueType && !toType.IsEnum))
                {
                    result = (T)Convert.ChangeType(source, toType, cultureInfo ?? CultureInfo.InvariantCulture);
                    return true;
                }

                if(toType.IsEnum && source is string)
                {
                    Enum.TryParse(source.ToString(), true, out result);
                    return true;
                }

                if(toType.IsEnum && source is int)
                {
                    result = ToEnum<T>((int)source);
                    return true;
                }

                result = (T)source;
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
            catch(InvalidCastException)
            {
                result = default;
                return false;
            }
        }

        private static TEnum ToEnum<TEnum>(this int val)
            where TEnum : struct//, IComparable, IFormattable, IConvertible
        {
            if(!typeof(TEnum).IsEnum)
            {
                return default(TEnum);
            }

            if(Enum.IsDefined(typeof(TEnum), val))
            {//if a straightforward single value, return that
                return (TEnum)Enum.ToObject(typeof(TEnum), val);
            }

            var candidates = Enum
                .GetValues(typeof(TEnum))
                .Cast<int>()
                .ToList();

            var isBitwise = candidates
                .Select((n, i) =>
                {
                    if(i < 2)
                    {
                        return n == 0 || n == 1;
                    }

                    return n / 2 == candidates[i - 1];
                })
                .All(y => y);

            var maxPossible = candidates.Sum();

            if(
                Enum.TryParse(val.ToString(), out TEnum asEnum)
                && (val <= maxPossible || !isBitwise)
            )
            {//if it can be parsed as a bitwise enum with multiple flags,
             //or is not bitwise, return the result of TryParse
                return asEnum;
            }

            //If the value is higher than all possible combinations,
            //remove the high imaginary values not accounted for in the enum
            var excess = Enumerable
                .Range(0, 32)
                .Select(n => (int)Math.Pow(2, n))
                .Where(n => n <= val && n > 0 && !candidates.Contains(n))
                .Sum();

            return Enum.TryParse((val - excess).ToString(), out asEnum) ? asEnum : default(TEnum);
        }
    }
}