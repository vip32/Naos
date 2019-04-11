namespace Naos.Core.Common
{
    using System;
    using System.Linq;
    using System.Text;

    public static partial class RandomGenerator
    {
        //private static readonly Random Random = new Random();
        private static readonly char[] Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToArray();
        private static readonly char[] CharactersLower = "abcdefghijklmnopqrstuvwxyz".ToArray();
        private static readonly char[] CharactersWithAlpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToArray();
        private static readonly char[] CharactersLowerWithAlpha = "abcdefghijklmnopqrstuvwxyz0123456789".ToArray();

        /// <summary>
        /// Generates a random string
        /// </summary>
        /// <param name="length">Lenght of the string</param>
        /// <param name="alphanumeric">String should also contain alphanumeric characters (0..9)</param>
        /// <returns></returns>
        public static string GenerateStringFast(int length, bool alphanumeric = true, bool lowerCase = false)
        {
            if(length < 0)
            {
                length = 0;
            }

            var characters = alphanumeric
                ? lowerCase ? CharactersLowerWithAlpha : CharactersWithAlpha
                : lowerCase ? CharactersLower : Characters;

            var sb = new StringBuilder(length);
            for(var i = 0; i < length; i++)
            {
                sb.Append(characters[Random.Next(0, characters.Length)]);
            }

            return sb.ToString();

            // slower
            //return new string(
            //    Enumerable.Range(1, length).Select(_ => characters[Random.Next(characters.Length)]).ToArray());
        }
    }
}