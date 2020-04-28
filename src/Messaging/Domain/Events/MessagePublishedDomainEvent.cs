namespace Naos.Messaging.Domain
{
    using Naos.Foundation.Domain;

    public class MessagePublishedDomainEvent : DomainEventBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePublishedDomainEvent"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MessagePublishedDomainEvent(Message message)
        {
            this.Message = message;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public Message Message { get; set; }
    }
}
