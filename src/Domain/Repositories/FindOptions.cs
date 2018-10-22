namespace Naos.Core.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Various options to specify the <see cref="IRepository{T}" /> find operations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Naos.Core.Domain.IFindOptions{T}" />
    public class FindOptions<T> : IFindOptions<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FindOptions{T}"/> class.
        /// </summary>
        /// <param name="skip">The skip amount.</param>
        /// <param name="take">The take amount.</param>
        public FindOptions(int? skip = null, int? take = null)
        {
            this.Take = take;
            this.Skip = skip;
        }

        public int? Take { get; set; }

        public int? Skip { get; set; }

        public IEnumerable<Expression<Func<T, object>>> Includes { get; }
    }
}