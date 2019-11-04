namespace Naos.Tracing.Infrastructure.Mongo
{
    using System;
    using Microsoft.Extensions.Logging;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Bson.Serialization.IdGenerators;
    using Naos.Foundation.Infrastructure;
    using Newtonsoft.Json;

    public class MongoLogTrace : IMongoEntity<object>
    {
        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonIgnore]
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public object Id { get; set; }

        /// <summary>
        /// Gets the identifier value.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
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

        // trace specific properties

        [JsonProperty(LogPropertyKeys.TrackName)]
        public string ns_trknm { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.TrackId)]
        public string ns_trkid { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.TrackTraceId)]
        public string ns_trktid { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.TrackParentId)]
        public string ns_trkpid { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.TrackKind)]
        public string ns_trkknd { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.TrackStatus)]
        public string ns_trksts { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.TrackStatusDescription)]
        public string ns_trkstd { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.TrackStartTime)]
        public string ns_trkstt { get; set; } // or use jsonproperty()?

        [JsonProperty(LogPropertyKeys.TrackEndTime)]
        public string ns_trkent { get; set; } // or use jsonproperty()?
    }
}
