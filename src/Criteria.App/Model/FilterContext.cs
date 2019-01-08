namespace Naos.Core.Filtering.App
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides access to the request correlation properties.
    /// </summary>
    public class FilterContext
    {
        public IEnumerable<Criteria> Criterias { get; set; }

        public IEnumerable<OrderBy> OrderBy { get; set; }

        public int? Skip { get; set; }

        public int? Take { get; set; }
    }
}
