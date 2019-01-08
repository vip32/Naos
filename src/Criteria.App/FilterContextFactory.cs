namespace Naos.Core.Filtering.App
{
    using System.Collections.Generic;
    using System.Linq;

    /// <inheritdoc />
    public class FilterContextFactory : IFilterContextFactory
    {
        private readonly IFilterContextAccessor accessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterContextFactory" /> class.
        /// </summary>
        public FilterContextFactory()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterContextFactory"/> class.
        /// </summary>
        /// <param name="accessor">The <see cref="IFilterContextAccessor"/> through which the <see cref="FilterContext"/> will be set.</param>
        public FilterContextFactory(IFilterContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        /// <inheritdoc />
        public FilterContext Create(IEnumerable<Criteria> criterias, IEnumerable<OrderBy> orderBy, int? skip = null, int? take = null)
        {
            var result = new FilterContext
            {
                Criterias = criterias ?? Enumerable.Empty<Criteria>(),
                OrderBy = orderBy ?? Enumerable.Empty<OrderBy>(),
                Skip = skip,
                Take = take
            };

            if (this.accessor != null)
            {
                this.accessor.Context = result;
            }

            return result;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.accessor != null)
            {
                this.accessor.Context = null;
            }
        }
    }
}