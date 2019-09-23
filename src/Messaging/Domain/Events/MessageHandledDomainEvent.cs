namespace Naos.Messaging.Domain
{
    using Naos.Foundation.Domain;

    public class MessageHandledDomainEvent : DomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandledDomainEvent"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageScope">The messageScope (servicename).</param>
        public MessageHandledDomainEvent(Message message, string messageScope)
        {
            this.Message = message;
            this.MessageScope = messageScope;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public Message Message { get; set; }

        public string MessageScope { get; }
    }
}
