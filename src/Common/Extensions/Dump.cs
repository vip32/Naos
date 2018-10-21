namespace Naos.Core.Common
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    public static partial class Extensions
    {
        private static JsonSerializerSettings CamelCasedJsonSettings => new JsonSerializerSettings
        {
            // PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[]
            {
                new StringEnumConverter(),
                new IsoDateTimeConverter(),
            }
        };

        /// <summary>
        ///     Dumps the object as a json string
        ///     Can be used for logging object contents.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="source">The object to dump. Can be null</param>
        /// <param name="indent">To indent the result or not</param>
        /// <param name="settings">The settings.</param>
        /// <returns>
        ///     the a string representing the object content
        /// </returns>
        public static string Dump<T>(this T source, bool indent = false, JsonSerializerSettings settings = null)
        {
            if (EqualityComparer<T>.Default.Equals(source, default(T)))
            {
                return string.Empty;
            }

            if (settings == null)
            {
                settings = CamelCasedJsonSettings;
            }

            return JsonConvert.SerializeObject(
                source,
                indent ? Formatting.Indented : Formatting.None,
                settings);
        }
    }
}
