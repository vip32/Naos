namespace Naos.Foundation.Domain
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;

    public class OrderOption<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public OrderOption(
            Expression<Func<TEntity, object>> expression,
            OrderDirection direction = OrderDirection.Ascending)
        {
            EnsureArg.IsNotNull(expression, nameof(expression));

            this.Expression = expression;
            this.Direction = direction;
        }

        public Expression<Func<TEntity, object>> Expression { get; set; }

        public OrderDirection Direction { get; set; }
    }
}