namespace Naos.Core.Domain.Repositories
{
    /// <summary>
    /// Various options for the <see cref="IGenericRepository{TEntity}"/>.
    /// </summary>
    /// <seealso cref="Domain.IRepositoryOptions" />
    public class RepositoryOptions : IRepositoryOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryOptions"/> class.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <param name="publishEvents">if set to <c>true</c> [publish events].</param>
        public RepositoryOptions(IEntityMapper mapper = null, bool publishEvents = true)
        {
            this.Mapper = mapper;
            this.PublishEvents = publishEvents;
        }

        /// <summary>
        /// Gets or sets the mapper.
        /// </summary>
        /// <value>
        /// The mapper.
        /// </value>
        public IEntityMapper Mapper { get; set; }

        public bool PublishEvents { get; set; }
    }
}