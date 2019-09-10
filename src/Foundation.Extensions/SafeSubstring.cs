namespace Naos.Foundation
{
    using System.Linq;

    public static partial class Extensions
    {
        public static string SafeSubstring(this string value, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return new string((value ?? string.Empty).Skip(startIndex).Take(length).ToArray());
        }
    }
}
