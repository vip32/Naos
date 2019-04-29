namespace Naos.Core.Common.Web
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    ///     Extends the HeaderDictionary type.
    /// </summary>
    public static class HeaderDictionaryExtensions
    {
        public static string GetValue(this IHeaderDictionary source, string name)
        {
            if(source == null || string.IsNullOrEmpty(name))
            {
                return default;
            }

            return !source.TryGetValue(name, out var keys) ? default : keys[0];
        }

        public static StringValues GetValues(this IHeaderDictionary source, string name)
        {
            if(source == null || string.IsNullOrEmpty(name))
            {
                return default;
            }

            return !source.TryGetValue(name, out var values) ? default : values;
        }
    }
}