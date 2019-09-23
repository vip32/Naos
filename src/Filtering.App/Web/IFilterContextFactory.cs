namespace Naos.RequestFiltering.App.Web
{
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A factory for creating and disposing an instance of a <see cref="FilterContext"/>.
    /// </summary>
    public interface IFilterContextFactory
    {
        /// <summary>
        /// Creates a new <see cref="FilterContext"/> with the for the current request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="criteriaQueryStringKey">The criteria query string key.</param>
        /// <param name="orderQueryStringKey">The order by query string key.</param>
        /// <param name="skipQueryStringKey">The skip query string key.</param>
        /// <param name="takeQueryStringKey">The take query string key.</param>
        FilterContext Create(
            HttpRequest request,
            string criteriaQueryStringKey = QueryStringKeys.Criteria,
            string orderQueryStringKey = QueryStringKeys.Order,
            string skipQueryStringKey = QueryStringKeys.Skip,
            string takeQueryStringKey = QueryStringKeys.Take);

        /// <summary>
        /// Disposes of the <see cref="FilterContext"/> for the current request.
        /// </summary>
        void Dispose();
    }
}
