namespace Naos.Foundation.Domain
{
    public class EntityInsertDomainEvent : DomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityInsertDomainEvent"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public EntityInsertDomainEvent(IEntity entity)
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
