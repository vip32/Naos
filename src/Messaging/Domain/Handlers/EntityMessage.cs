namespace Naos.Core.Messaging.Domain
{
    using Naos.Core.Domain;

    public class EntityMessage<T> : Message
        where T : class, IEntity
    {
        /// <summary>
        /// Gets or sets the domain entity (based on string id).
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public T Entity { get; set; }
    }
}