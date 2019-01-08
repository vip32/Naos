namespace Naos.Core.Filtering.App
{
    using System.Collections.Generic;

    /// <summary>
    /// A factory for creating and disposing an instance of a <see cref="FilterContext"/>.
    /// </summary>
    public interface IFilterContextFactory
    {
        /// <summary>
        /// Creates a new <see cref="FilterContext"/> with the for the current request.
        /// </summary>
        /// <param name="criterias"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns>A new instance of a <see cref="CorrelationContext"/>.</returns>
        FilterContext Create(IEnumerable<Criteria> criterias, IEnumerable<OrderBy> orderBy, int? skip = null, int? take = null);

        /// <summary>
        /// Disposes of the <see cref="FilterContext"/> for the current request.
        /// </summary>
        void Dispose();
    }
}
