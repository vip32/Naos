namespace Naos.Foundation
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    public static class RandomGenerator
    {
        private static readonly Random Random = new Random();
        private static readonly char[] CharactersUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToArray();
        private static readonly char[] CharactersLower = "abcdefghijklmnopqrstuvwxyz".ToArray();
        private static readonly char[] CharactersUpperWithAlpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToArray();
        private static readonly char[] CharactersLowerWithAlpha = "abcdefghijklmnopqrstuvwxyz0123456789".ToArray();
        private static readonly char[] CharactersNumber = "1234567890".ToArray();
        private static readonly char[] CharactersNonAlpha = "!@#$%^&*_-=+".ToArray();

        /// <summary>
        /// Generates a random string with letters and numerals.
        /// </summary>
        /// <param name="length">Lenght of the string.</param>
        /// <param name="alphanumeric">String should also contain alphanumeric characters (0..9).</param>
        public static string GenerateString(int length, bool alphanumeric = true, bool lowerCase = false)
        {
            if(length < 0)
            {
                length = 0;
            }

            var characters = alphanumeric
                ? lowerCase ? CharactersLowerWithAlpha : CharactersUpperWithAlpha
                : lowerCase ? CharactersLower : CharactersUpper;

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

        /// <summary>
        /// Generates a random string with with letters, numerals and optional special characters.
        /// </summary>
        /// <param name="length">Lenght of the string.</param>
        /// <param name="alphanumeric">String should also contain alphanumeric characters (0..9).</param>
        /// <param name="lowerCase">Only lowercase characters.</param>
        /// <param name="nonAlphanumeric">String should also contain non alphanumeric characters (!\"§$%&/()=?*#-.,).</param>
        /// <param name="mixedCase">Mixed lowercase and uppercase characters.</param>
        public static string GenerateComplexString(int length, bool alphanumeric = true, bool lowerCase = false, bool nonAlphanumeric = false, bool mixedCase = false)
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

        public static string GeneratePassword(int length = 12, bool nonAlphanumeric = true)
        {
            var bytes = new byte[length];
            new RNGCryptoServiceProvider().GetBytes(bytes); //  source of randomness is the crypto RNG
            var result = new StringBuilder();

            foreach(var @byte in bytes)
            {
                // randomly select a character class
                switch(Random.Next(4))
                {
                    case 0:
                        result.Append(CharactersLower[@byte % CharactersLower.Length]);
                        break;
                    case 1:
                        result.Append(CharactersUpper[@byte % CharactersUpper.Length]);
                        break;
                    case 2:
                        result.Append(CharactersNumber[@byte % CharactersNumber.Length]);
                        break;
                    case 3:
                        if(nonAlphanumeric)
                        {
                            result.Append(CharactersNonAlpha[@byte % CharactersNonAlpha.Length]);
                        }
                        else
                        {
                            result.Append(CharactersUpper[@byte % CharactersUpper.Length]);
                        }

                        break;
                }
            }

            return result.ToString();
        }
    }
}