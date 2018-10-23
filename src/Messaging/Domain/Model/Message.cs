namespace Naos.Core.Messaging.Domain.Model
{
    using System;

    public class Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        public Message()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets or sets the identifier of this message.
        /// </summary>
        /// <value>
        /// The message identifier.
        /// </value>
        public string Id { get; set; }

        public string EntityType => this.GetType().FullName;

        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        /// <value>
        /// The correlation identifier.
        /// </value>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the origin of this event instance.
        /// </summary>
        /// <value>
        /// The origin.
        /// </value>
        public string Origin { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public MessageStatus Status { get; set; }
    }
}
