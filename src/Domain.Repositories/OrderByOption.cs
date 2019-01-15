namespace Naos.Core.Domain.Repositories
{
    using System;
    using System.Linq.Expressions;

    public class OrderByOption<TEntity>
    {
        public OrderByDirection Direction { get; set; }

        public Expression<Func<TEntity, object>> Expression { get; set; }
    }
}