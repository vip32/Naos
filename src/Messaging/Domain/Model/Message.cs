namespace Naos.Core.Messaging.Domain
{
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Newtonsoft.Json;

    public class Message
        : IEntity<string>, IHaveDiscriminator, IAggregateRoot // TODO: really need this? or just like DomainEvent (=clean)
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        public Message()
        {
            this.Id = IdGenerator.Instance.Next;
            this.Identifier = RandomGenerator.GenerateString(5, false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        public Message(string id, string correlationId)
        {
            this.Id = id ?? IdGenerator.Instance.Next;
            this.CorrelationId = correlationId;
            this.Identifier = RandomGenerator.GenerateString(5, false);
        }

        /// <summary>
        /// Gets or sets the identifier of this message.
        /// </summary>
        /// <value>
        /// The message identifier.
        /// </value>
        [JsonIgnore] // needed for FolderBasedBroker::Publish (JsonSerialize ID issue)
        public string Id { get; set; }

        [JsonProperty(PropertyName = "id")]
        object IEntity.Id
        {
            get { return this.Id; }
            set { this.Id = (string)value; }
        }

        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        /// <value>
        /// The correlation identifier.
        /// </value>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the short identifier for this message.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Gets the type of the entity (discriminator).
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public string Discriminator => this.GetType().FullName;

        /// <summary>
        /// Gets or sets the origin service name of this <see cref="Message"/> instance.
        /// </summary>
        /// <value>
        /// The origin.
        /// </value>
        public string Origin { get; set; } // Product.Capability

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public MessageStatus Status { get; set; }

        public DomainEvents DomainEvents => new DomainEvents();
    }
}
