namespace Naos.Core.App.Filtering.App.Web
{
    /// <summary>
    /// Options for criteria.
    /// </summary>
    public class FilterMiddlewareOptions
    {
        private const string DefaultCriteriaQueryStringKey = "q";
        private const string DefaultOrderByQueryStringKey = "order";
        private const string DefaultSkipQueryStringKey = "skip";
        private const string DefaultTakeQueryStringKey = "take";

        /// <summary>
        /// The name of the querystring part from which the criterias are read.
        /// </summary>
        public string CriteriaQueryStringKey { get; set; } = DefaultCriteriaQueryStringKey;

        /// <summary>
        /// The name of the querystring part from which the orderby clause is read.
        /// </summary>
        public string OrderByQueryStringKey { get; set; } = DefaultOrderByQueryStringKey;

        /// <summary>
        /// The name of the querystring part from which the skip clause is read.
        /// </summary>
        public string SkipQueryStringKey { get; set; } = DefaultSkipQueryStringKey;

        /// <summary>
        /// The name of the querystring part from which the take clause is read.
        /// </summary>
        public string TakeQueryStringKey { get; set; } = DefaultTakeQueryStringKey;
    }
}
