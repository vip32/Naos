namespace Naos.Foundation.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Various options to specify the <see cref="IGenericRepository{TEntity}"/> find operations.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IFindOptions<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
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
        /// Gets or sets the ordering.
        /// </summary>
        /// <value>
        /// The ordering.
        /// </value>
        OrderOption<TEntity> Order { get; set; }

        /// <summary>
        /// Gets or sets the ordersings.
        /// </summary>
        /// <value>
        /// The ordersings.
        /// </value>
        IEnumerable<OrderOption<TEntity>> Orders { get; set; }

        /// <summary>
        /// Gets or sets the includes.
        /// </summary>
        /// <value>
        /// The includes.
        /// </value>
        IEnumerable<Expression<Func<TEntity, object>>> Includes { get; set; }

        /// <summary>
        /// Determines whether this instance has orderings.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance has orderings; otherwise, <c>false</c>.
        /// </returns>
        bool HasOrders();
    }
}