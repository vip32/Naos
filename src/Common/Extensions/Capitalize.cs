namespace Naos.Core.Common
{
    using System.Diagnostics;
    using System.Text;

    public static partial class Extensions
    {
        /// <summary>
        /// Ensures the first character is a capital
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string Capitalize(this string source)
        {
            if(source.IsNullOrEmpty())
            {
                return source;
            }

            var result = new StringBuilder();
            for(var i = 0; i < source.Length; i++)
            {
                result.Append(i == 0 ? char.ToUpper(source[i]) : source[i]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Ensures the first character is a not a capital
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string Decapitalize(this string source)
        {
            if(source.IsNullOrEmpty())
            {
                return source;
            }

            var result = new StringBuilder();
            for(var i = 0; i < source.Length; i++)
            {
                result.Append(i == 0 ? char.ToLower(source[i]) : source[i]);
            }

            return result.ToString();
        }
    }
}
