namespace Naos.Core.Common
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public static partial class Extensions
    {
        /// <summary>
        /// Dumps the object as a json string
        /// Can be used for logging object contents.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="source">The object to dump. Can be null.</param>
        /// <param name="indent">To indent the result or not.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>the a string representing the object content.</returns>
        public static string Dump<T>(
            this T source,
            bool indent = false,
            JsonSerializerSettings settings = null)
             where T : class
        {
            if(EqualityComparer<T>.Default.Equals(source, default))
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(
                source,
                indent ? Formatting.Indented : Formatting.None,
                settings ?? DefaultJsonSerializerSettings.Create());
        }
    }
}
