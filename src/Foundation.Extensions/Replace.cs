namespace Naos.Foundation
{
    using System;
    using System.Text;

    public static partial class Extensions
    {
        // Summary:
        //     Returns a new string in which all occurrences of a specified Unicode character
        //     in this instance are replaced with another specified Unicode character.
        //
        // Parameters:
        //   oldChar:
        //     The Unicode character to be replaced.
        //
        //   newChar:
        //     The Unicode character to replace all occurrences of oldChar.
        //
        // Returns:
        //     A string that is equivalent to this instance except that all instances of oldChar
        //     are replaced with newChar. If oldChar is not found in the current instance, the
        //     method returns the current instance unchanged.
        public static string Replace(this string source, string oldValue, string newValue, StringComparison comparison)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return source;
            }

            // skip the loop entirely if oldValue and newValue are the same
            if (string.Compare(oldValue, newValue, comparison) == 0)
            {
                return source;
            }

            // this is a hack to avoid the bug reported here https://stackoverflow.com/questions/244531/is-there-an-alternative-to-string-replace-that-is-case-insensitive/13847351#comment31063745_244933
            if (oldValue.Length > source.Length)
            {
                return source;
            }

            var sb = new StringBuilder();

            var previousIndex = 0;
            var index = source.IndexOf(oldValue, comparison);

            while (index != -1)
            {
                sb.Append(source.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = source.IndexOf(oldValue, index, comparison);
            }

            sb.Append(source.Substring(previousIndex));

            return sb.ToString();
        }
    }
}
