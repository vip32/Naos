namespace Naos.Core.Common
{
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    public static partial class Extensions
    {
        [DebuggerStepThrough]
        public static bool IsBase64(this string source)
        {
            source = source.Trim();
            return (source.Length % 4 == 0) && Regex.IsMatch(source, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }
    }
}