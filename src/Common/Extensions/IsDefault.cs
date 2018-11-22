namespace Naos.Core.Common
{
    using System;

    public static partial class Extensions
    {
        //// <summary>
        //// Determines whether [is default].
        //// </summary>
        //// <typeparam name="T"></typeparam>
        //// <param name="source">The value.</param>
        //// <returns>
        ////   <c>true</c> if [is null or default] [the specified value]; otherwise, <c>false</c>.
        //// </returns>
        //public static bool IsDefault<T>(this T source)
        //{
        //    if (typeof(T).IsValueType)
        //    {
        //        return source == Activator.CreateInstance(typeof(T));
        //    }

        //    return EqualityComparer<T>.Default.Equals(source, default);
        //}

        public static bool IsDefault(this object source)
        {
            if (source?.GetType().IsValueType == true)
            {
                switch (source)
                {
                    case int s:
                        return s == default;
                    case long s:
                        return s == default;
                    case double s:
                        return s == default;
                    case decimal s:
                        return s == default;
                    case Guid s:
                        return s == default;
                    // etc: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/value-types
                    default:
                        throw new NotSupportedException($"IsDefault for type {source.GetType().Name}");
                }
            }

            return source == null;
        }

        public static bool IsDefault(this string source)
        {
            return source == default;
        }

        public static bool IsDefault(this int source)
        {
            return source == default;
        }

        public static bool IsDefault(this Guid source)
        {
            return source == default;
        }

        public static bool IsDefault(this DateTime source)
        {
            return source == default;
        }
    }
}
