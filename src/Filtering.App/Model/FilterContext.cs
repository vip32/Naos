namespace Naos.Core.Filtering.App
{
    using System.Collections.Generic;
    using Naos.Core.Common;

    /// <summary>
    /// Provides access to the request correlation properties.
    /// </summary>
    public class FilterContext
    {
        public IEnumerable<Criteria> Criterias { get; set; }

        public IEnumerable<Order> OrderBy { get; set; }

        public int? Skip { get; set; }

        public int? Take { get; set; }

        public bool Enabled
            => !this.Criterias.IsNullOrEmpty() || !this.OrderBy.IsNullOrEmpty() || this.Skip.HasValue || this.Take.HasValue;
    }
}
