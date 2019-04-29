namespace Naos.Core.Common
{
    using System;
    using System.IO;
    using EnsureThat;

    /// <summary>
    /// Base 64 encoder using URL and filename-safe alphabet.
    /// </summary>
    public static class Base64
    {
        /// <summary>
        /// Encodes the given <see langword="byte"/>[] into a URL and filename-safe Base64 encoded string.
        /// </summary>
        /// <param name="input">The argument to encode.</param>
        /// <returns>Encoded result as string.</returns>
        public static string Encode(byte[] input)
        {
            var s = Convert.ToBase64String(input); // Standard base64 encoder

            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding

            return s;
        }

        /// <summary>
        /// Decodes the given URL and filename-safe Base64 string into a
        /// <see langword="byte"/>[].
        /// </summary>
        /// <param name="input">The argument to decode.</param>
        /// <returns>Decoded result as <see langword="byte"/>[].</returns>
        /// <exception cref="InvalidDataException">Thrown when the given
        /// <paramref name="input"/> is not a valid Base64 encoded string.
        /// </exception>
        public static byte[] Decode(string input)
        {
            EnsureArg.IsNotNullOrWhiteSpace(input, nameof(input));

            var s = input;
            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding

            // Pad with trailing '='s
            switch(s.Length % 4)
            {
                case 0: break; // No pad chars in this case
                case 2:
                    s += "==";
                    break; // Two pad chars
                case 3:
                    s += "=";
                    break; // One pad char
                default: throw new InvalidDataException("Invalid Base64UrlSafe encoded string.");
            }

            return Convert.FromBase64String(s); // Standard base64 decoder
        }
    }
}