namespace Naos.Foundation.Infrastructure
{
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class EntityFrameworkRepositoryOptions : BaseOptions
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
    }
}