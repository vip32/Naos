namespace Naos.Tracing.Infrastructure.Mongo
{
    using System;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Infrastructure;
    using Newtonsoft.Json;

    public class MongoLogTrace : IMongoEntity
    {
        public string Id { get; set; }

        public string Level { get; set; }

        public DateTime Timestamp { get; set; }

        public string RenderedMessage { get; set; }

        //public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        public MongoLogTraceProperties Properties { get; set; }
    }

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable IDE1006 // Naming Styles
    public class MongoLogTraceProperties
    {
        [JsonProperty(LogPropertyKeys.Ticks)]
        public long ns_ticks { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.ServiceName)]
        public long ns_svcname { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.ServiceProduct)]
        public long ns_svcprod { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.ServiceCapability)]
        public long ns_svccapa { get; set; } // or use jsonproperty()?
    }
}
