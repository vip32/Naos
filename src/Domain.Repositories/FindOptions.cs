namespace Naos.Core.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Naos.Core.Common;

    /// <summary>
    /// Various options to specify the <see cref="IRepository{T}" /> find operations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Domain.IFindOptions{TEntity}" />
    public class FindOptions<T> : IFindOptions<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FindOptions{T}"/> class.
        /// </summary>
        public FindOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FindOptions{T}"/> class.
        /// </summary>
        /// <param name="skip">The skip amount.</param>
        /// <param name="take">The take amount.</param>
        /// <param name="order">The order option</param>
        /// <param name="orderExpression">the order expresion</param>
        /// <param name="orders">The order options</param>
        public FindOptions(int? skip = null, int? take = null, OrderOption<T> order = null, Expression<Func<T, object>> orderExpression = null, IEnumerable<OrderOption<T>> orders = null)
        {
            this.Take = take;
            this.Skip = skip;
            this.Order = orderExpression != null ? new OrderOption<T>(orderExpression) : order;
            this.Orders = orders;
        }

        public int? Skip { get; set; }

        public int? Take { get; set; }

        public OrderOption<T> Order { get; set; }

        public IEnumerable<OrderOption<T>> Orders { get; set; }

        public IEnumerable<Expression<Func<T, object>>> Includes { get; set; }

        public bool HasOrders() => this.Order != null || this.Orders.SafeAny();
    }
}