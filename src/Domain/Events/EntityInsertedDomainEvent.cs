namespace Naos.Core.Domain
{
    public class EntityInsertedDomainEvent : DomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityInsertedDomainEvent"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public EntityInsertedDomainEvent(IEntity entity)
        {
            this.Entity = entity;
        }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public IEntity Entity { get; set; }
    }
}
