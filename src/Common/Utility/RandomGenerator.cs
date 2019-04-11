namespace Naos.Core.Common
{
    using System;
    using System.Linq;

    public static partial class RandomGenerator
    {
        private static readonly Random Random = new Random();

        /// <summary>
        /// Generates a random string
        /// </summary>
        /// <param name="length">Lenght of the string</param>
        /// <param name="alphanumeric">String should also contain alphanumeric characters (0..9)</param>
        /// <param name="nonAlphanumeric">String should also contain non alphanumeric characters (!\"§$%&/()=?*#-.,)</param>
        /// <param name="lowerCase">Only lowercase characters</param>
        /// <param name="mixedCase">Mixed lowercase and uppercase characters</param>
        /// <returns></returns>
        public static string GenerateString(int length, bool alphanumeric = true, bool nonAlphanumeric = false, bool lowerCase = false, bool mixedCase = false)
        {
            if(length < 0)
            {
                length = 0;
            }

            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var chars = alphanumeric
                ? characters + "0123456789"
                : characters;

            if(nonAlphanumeric)
            {
                chars += "!\"§$%&/()=?*#-.,";
            }

            if(lowerCase)
            {
                chars = chars.ToLower();
            }

            if(!lowerCase && mixedCase)
            {
                chars += characters.ToLower();
            }

            return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}