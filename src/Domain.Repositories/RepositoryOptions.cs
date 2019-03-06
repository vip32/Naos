namespace Naos.Core.Domain.Repositories
{
    using Naos.Core.Common.Mapping;

    /// <summary>
    /// Various options for the <see cref="IRepository{TEntity}"/>
    /// </summary>
    /// <seealso cref="Domain.IRepositoryOptions" />
    public class RepositoryOptions : IRepositoryOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryOptions"/> class.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <param name="publishEvents">if set to <c>true</c> [publish events].</param>
        public RepositoryOptions(IMapper mapper = null, bool publishEvents = true)
        {
            this.Mapper = mapper;
            this.PublishEvents = publishEvents;
        }

        public IMapper Mapper { get; set; }

        public bool PublishEvents { get; set; }
    }
}