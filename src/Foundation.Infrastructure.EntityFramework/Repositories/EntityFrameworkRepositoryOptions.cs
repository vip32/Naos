namespace Naos.Foundation.Infrastructure
{
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Naos.Foundation.Domain;

    public class EntityFrameworkRepositoryOptions : OptionsBase
    {
        /// <summary>
        /// Gets or sets the mediator.
        /// </summary>
        /// <value>
        /// The mediator.
        /// </value>
        public IMediator Mediator { get; set; }

        public bool PublishEvents { get; set; } = true; // Obsolete > optional decorator

        /// <summary>
        /// Gets or sets the database context.
        /// </summary>
        /// <value>
        /// The database context.
        /// </value>
        public DbContext DbContext { get; set; }

        public IEntityMapper Mapper { get; set; }
    }
}