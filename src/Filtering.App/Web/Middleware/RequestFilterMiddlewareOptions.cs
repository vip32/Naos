namespace Naos.RequestFiltering.App.Web
{
    /// <summary>
    /// Options for criteria.
    /// </summary>
    public class RequestFilterMiddlewareOptions
    {
        /// <summary>
        /// The name of the querystring part from which the criterias are read.
        /// </summary>
        public string CriteriaQueryStringKey { get; set; } = QueryStringKeys.Criteria;

        /// <summary>
        /// The name of the querystring part from which the orderby clause is read.
        /// </summary>
        public string OrderQueryStringKey { get; set; } = QueryStringKeys.Order;

        /// <summary>
        /// The name of the querystring part from which the skip clause is read.
        /// </summary>
        public string SkipQueryStringKey { get; set; } = QueryStringKeys.Skip;

        /// <summary>
        /// The name of the querystring part from which the take clause is read.
        /// </summary>
        public string TakeQueryStringKey { get; set; } = QueryStringKeys.Take;
    }
}
