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

        /// <summary>
        /// Gets or sets the skip amount.
        /// </summary>
        /// <value>
        /// The skip.
        /// </value>
        public int? Skip { get; set; }

        /// <summary>
        /// Gets or sets the take amount.
        /// </summary>
        /// <value>
        /// The take.
        /// </value>
        public int? Take { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public OrderOption<T> Order { get; set; }

        /// <summary>
        /// Gets or sets the orders.
        /// </summary>
        /// <value>
        /// The orders.
        /// </value>
        public IEnumerable<OrderOption<T>> Orders { get; set; }

        /// <summary>
        /// Gets or sets the includes.
        /// </summary>
        /// <value>
        /// The includes.
        /// </value>
        public IEnumerable<Expression<Func<T, object>>> Includes { get; set; }

        /// <summary>
        /// Determines whether this instance has orders.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance has orders; otherwise, <c>false</c>.
        /// </returns>
        public bool HasOrders() => this.Order != null || this.Orders.SafeAny();
    }
}