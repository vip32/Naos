namespace Naos.Foundation
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    public static class TypedJsonSerializerSettings
    {
        public static JsonSerializerSettings Create()
        {
            return new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
#pragma warning disable SCS0028 // TypeNameHandling is set to other value than 'None' that may lead to deserialization vulnerability
                TypeNameHandling = TypeNameHandling.All,
                //DateParseHandling = DateParseHandling.DateTimeOffset,
                //DateFormatHandling = DateFormatHandling.IsoDateFormat,
                //DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                Converters = new List<JsonConverter>
                {
                    //new GuidConverter(),
                    new StringEnumConverter(),
                    new IsoDateTimeConverter
                    {
                        DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ" // utc, no timezone offset (+0:00)
                    }
                }
            };
        }
    }
}