namespace Naos.Core.RequestFiltering.App
{
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    /// <summary>
    /// Provides access to the filter information.
    /// </summary>
    public class FilterContext
    {
        public IEnumerable<Criteria> Criterias { get; set; } = Enumerable.Empty<Criteria>();

        public IEnumerable<Order> Orders { get; set; } = Enumerable.Empty<Order>();

        public int? Skip { get; set; }

        public int? Take { get; set; }

        public bool Enabled
            => !this.Criterias.IsNullOrEmpty() || !this.Orders.IsNullOrEmpty() || this.Skip.HasValue || this.Take.HasValue;

        //// <summary>
        //// Converts the <see cref="FilterContext"/> to a list of specifications
        //// </summary>
        //// <typeparam name="T">The type of the entity.</typeparam>
        //// <returns></returns>
        public IEnumerable<Specification<T>> GetSpecifications<T>()
        {
            return SpecificationsFactory.Create<T>(this);
        }

        //// <summary>
        //// Converts the <see cref="FilterContext"/> to a <see cref="IFindOptions<T>"/>
        //// </summary>
        //// <typeparam name="T">The type of the entity.</typeparam>
        //// <returns></returns>
        public IFindOptions<T> GetFindOptions<T>()
        {
            return FindOptionsFactory.Create<T>(this);
        }
    }
}
