namespace Naos.Core.Messaging
{
    using Domain.Model;
    using Naos.Core.Domain;

    public class EntityMessage<TEntity> : Message
        where TEntity : Entity<string>
    {
        /// <summary>
        /// Gets or sets the domain entity (based on string id).
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public TEntity Entity { get; set; }
    }
}