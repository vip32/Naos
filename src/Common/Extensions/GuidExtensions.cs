namespace Naos.Core.Common
{
    using System;
    using System.Diagnostics;

    public static partial class Extensions
    {
        /// <summary>
        /// Returns a <c>Base64</c> encoded <see cref="Guid"/>.
        /// <example>
        /// DRfscsSQbUu8bXRqAvcWQA== or DRfscsSQbUu8bXRqAvcWQA depending on <paramref name="trimEnd"/>.
        /// </example>
        /// <remarks>
        /// The result of this method is not <c>URL</c> safe.
        /// See: <see href="https://blog.codinghorror.com/equipping-our-ascii-armor/"/>
        /// </remarks>
        /// </summary>
        [DebuggerStepThrough]
        public static string ToBase64(this Guid source, bool trimEnd = false)
        {
            var result = Convert.ToBase64String(source.ToByteArray());
            return trimEnd ? result.Substring(0, result.Length - 2) : result;
        }

        /// <summary>
        /// Generates a maximum of 16 character, <see cref="Guid"/> based string with very little chance of collision.
        /// <example>3c4ebc5f5f2c4edc</example>.
        /// <remarks>
        /// The result of this method is <c>URL</c> safe.
        /// Slower than <see cref="ToBase64"/>.
        /// See: <see href="http://madskristensen.net/post/generate-unique-strings-and-numbers-in-c"/>
        /// </remarks>
        /// </summary>
        [DebuggerStepThrough]
        public static string ToCode(this Guid source)
        {
            long i = 1;
            foreach(var b in source.ToByteArray())
            {
                i *= b + 1;
            }

            return (i - DateTime.Now.Ticks).ToString("x");
        }

        /// <summary>
        /// Generates a 19 character, <see cref="Guid"/> based number.
        /// <example>4801539909457287012</example>.
        /// <remarks>
        /// Faster than <see cref="ToBase64"/>.
        /// See: <see href="http://madskristensen.net/post/generate-unique-strings-and-numbers-in-c"/>
        /// </remarks>
        /// </summary>
        [DebuggerStepThrough]
        public static long ToNumber(this Guid source)
            => BitConverter.ToInt64(source.ToByteArray(), 0);

        [DebuggerStepThrough]
        public static Guid? ToGuid(this string source)
        {
            if(source.IsNullOrEmpty())
            {
                return null;
            }

            if(source.IsBase64())
            {
                return new Guid(Convert.FromBase64String(source));
            }

            var result = Guid.TryParse(source, out var parsedResult);
            if(result)
            {
                return parsedResult;
            }

            return null;
        }
    }
}