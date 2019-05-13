namespace Naos.Core.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Various options to specify the <see cref="IGenericRepository{TEntity}"/> find operations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFindOptions<T>
    {
        /// <summary>
        /// Gets or sets the skip amount.
        /// </summary>
        /// <value>
        /// The skip.
        /// </value>
        int? Skip { get; set; }

        /// <summary>
        /// Gets or sets the take amount.
        /// </summary>
        /// <value>
        /// The take.
        /// </value>
        int? Take { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        OrderOption<T> Order { get; set; }

        /// <summary>
        /// Gets or sets the orders.
        /// </summary>
        /// <value>
        /// The orders.
        /// </value>
        IEnumerable<OrderOption<T>> Orders { get; set; }

        /// <summary>
        /// Gets or sets the includes.
        /// </summary>
        /// <value>
        /// The includes.
        /// </value>
        IEnumerable<Expression<Func<T, object>>> Includes { get; set; }

        /// <summary>
        /// Determines whether this instance has orders.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance has orders; otherwise, <c>false</c>.
        /// </returns>
        bool HasOrders();
    }
}