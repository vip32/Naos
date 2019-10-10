namespace Naos.Operations.Infrastructure.Mongo
{
    using System;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Infrastructure;
    using Newtonsoft.Json;

    public class MongoLogEvent : IMongoEntity<object>
    {
        //[JsonProperty("_id")]
        //public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonIgnore]
        public object Id { get; set; }

        /// <summary>
        /// Gets the identifier value.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        //[JsonProperty(PropertyName = "_id")]
        object IMongoEntity.Id
        {
            get { return this.Id; }
            set { this.Id = value; }
        }

        //[JsonProperty("Level")]
        public string Level { get; set; }

        //[JsonProperty("Timestamp")]
        public DateTime Timestamp { get; set; }

        //[JsonProperty("RenderedMessage")]
        public string RenderedMessage { get; set; }

        //public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        [JsonProperty("Properties")]
        public MongoLogEventProperties Properties { get; set; }
    }

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable IDE1006 // Naming Styles
    public class MongoLogEventProperties
    {
        [JsonProperty(LogPropertyKeys.CorrelationId)]
        public string ns_corid { get; set; } // or use jsonproperty()?

        [JsonProperty("sourceContext")]
        public string sourceContext { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.LogKey)]
        public string logKey { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.Environment)]
        public string ns_env { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.TrackType)]
        public string ns_trktyp { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.Ticks)]
        public long ns_ticks { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.ServiceName)]
        public string ns_svcname { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.ServiceProduct)]
        public string ns_svcprod { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.ServiceCapability)]
        public string ns_svccapa { get; set; } // or use jsonproperty()?
    }
}
