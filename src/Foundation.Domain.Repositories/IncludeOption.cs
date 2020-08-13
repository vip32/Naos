namespace Naos.Foundation.Domain
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;

    public class IncludeOption<TEntity>
        where TEntity : class, IEntity
    {
        public IncludeOption(
            Expression<Func<TEntity, object>> expression)
        {
            EnsureArg.IsNotNull(expression, nameof(expression));

            this.Expression = expression;
        }

        public IncludeOption(string path)
        {
            EnsureArg.IsNotNull(path, nameof(path));

            this.Path = path;
        }

        public Expression<Func<TEntity, object>> Expression { get; }

        public string Path { get; }
    }
}