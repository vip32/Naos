namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;

    public static partial class Extensions
    {
        /// <summary>
        /// Returns the property or a default value if the source was null.
        /// </summary>
        /// <typeparam name="T1">The type of the source.</typeparam>
        /// <typeparam name="TValue">The type of the property.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="property">The property to get.</param>
        public static TValue ValueOrDefault<T1, TValue>(this T1 source, Func<T1, TValue> property)
        {
            if(typeof(T1).IsValueType)
            {
                return Equals(source, default(T1)) ? default(TValue) : property(source);
            }

            return Equals(source, null) ? default(TValue) : property(source);
        }

        /// <summary>
        ///     Returns the property or a default value if the source was null.
        /// </summary>
        /// <typeparam name="T1">The type of the source.</typeparam>
        /// <typeparam name="TValue">The type of the property.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="property">The property to get.</param>
        /// <param name="defaultValue">The defaule value.</param>
        public static TValue ValueOrDefault<T1, TValue>(this T1 source, Func<T1, TValue> property, TValue defaultValue = default(TValue))
        {
            if(typeof(T1).IsValueType)
            {
                return Equals(source, default(T1)) ? defaultValue : property(source);
            }

            return Equals(source, null) ? defaultValue : property(source);
        }

        //public static bool ValueOrDefaultB<T1>(this T1 source, Func<T1, bool?> property, bool defaultValue = false)
        //{
        //    if (typeof(T1).IsValueType)
        //    {
        //        return Equals(source, default(T1)) ? defaultValue : property(source).Value;
        //    }

        //    return Equals(source, null) ? defaultValue : property(source).Value;
        //}

        public static TValue ValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            TKey key,
            TValue defaultValue = default(TValue))
        {
            if(source.IsNullOrEmpty())
            {
                return default(TValue);
            }

            return source.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}