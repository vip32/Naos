namespace Naos.Core.Tracing.Domain
{
    using System;
    using System.Collections.Generic;
    using Humanizer;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Newtonsoft.Json;

    public class LogTrace : IEntity<string>, IAggregateRoot
    {
        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonIgnore]
        public string Id { get; set; }

        /// <summary>
        /// Gets the identifier value.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty(PropertyName = "id")]
        object IEntity.Id
        {
            get { return this.Id; }
            set { this.Id = (string)value; }
        }

        public string Level { get; set; }

        public string Key { get; set; }

        public string Environment { get; set; }

        public string Message { get; set; }

        public DateTime Timestamp { get; set; }

        public long Ticks { get; set; }

        public string CorrelationId { get; set; }

        public string ServiceName { get; set; }

        public string ServiceProduct { get; set; }

        public string ServiceCapability { get; set; }

        public string SourceContext { get; set; }

        public string TrackType { get; set; }

        public string TrackId { get; set; }

        public string OperationName { get; set; }

        public string TraceId { get; set; }

        public string SpanId { get; set; }

        public string ParentSpanId { get; set; }

        public string Kind { get; set; }

        public string Status { get; set; }

        public string StatusDescription { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public TimeSpan Duration =>
            this.EndTime.HasValue && this.StartTime.HasValue ? this.EndTime.Value - this.StartTime.Value : TimeSpan.Zero;

        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> Tags { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> Logs { get; set; } = new Dictionary<string, object>();

        public DomainEvents DomainEvents => new DomainEvents();

        public string GetAge()
        {
            var timestamp = this.Timestamp;
            if (timestamp.IsDefault())
            {
                return string.Empty;
            }

            return (DateTime.UtcNow - this.Timestamp).Humanize();
        }

        public override string ToString()
        {
            return $"{this.Timestamp.ToUniversalTime():u} - {this.Message} ({this.SpanId}) -> took {this.Duration.Humanize()}";
        }
    }
}
