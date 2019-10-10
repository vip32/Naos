namespace Naos.Sample.Inventory.Infrastructure
{
    using System;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure;
    using Newtonsoft.Json;

    public class DtoProductReplenishment : IMongoEntity<string>
    {
        //public string Id { get; set; }

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
        //[JsonProperty(PropertyName = "_id")]
        object IMongoEntity.Id
        {
            get { return this.Id; }
            set { this.Id = (string)value; }
        }

        public string ProductNumber { get; set; }

        public int Amount { get; set; }

        public string Region { get; set; }

        public string OwnerId { get; set; }

        public string FromLocation { get; set; }

        public DateTimeOffset ShipDate { get; set; } = DateTimeOffset.UtcNow;

        public string AtLocation { get; set; }

        public DateTimeOffset ArriveDate { get; set; } = DateTimeOffset.UtcNow;

        public string IdentifierHash { get; set; }

        public State State { get; set; } = new State();
    }
}
