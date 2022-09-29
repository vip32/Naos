namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using EnsureThat;
    using Microsoft.EntityFrameworkCore;
    using Naos.Foundation.Domain;

    public static partial class Extensions
    {
        public static IQueryable<TEntity> IncludeIf<TEntity>(
            this IQueryable<TEntity> source,
            IFindOptions<TEntity> options)
            where TEntity : class, IEntity, IAggregateRoot
        {
            if (options == null || options?.HasIncludes() == false)
            {
                return source;
            }

            foreach (var include in (options.Includes ?? new List<IncludeOption<TEntity>>()).Insert(options.Include))
            {
                if (include.Expression != null)
                {
                    source = source.Include(include.Expression);
                }

                if (!include.Path.IsNullOrEmpty())
                {
                    source = source.Include(include.Path);
                }
            }

            return source;
        }

        public static IQueryable<TDestination> IncludeIf<TEntity, TDestination>(
            this IQueryable<TDestination> source,
            IFindOptions<TEntity> options,
            IEntityMapper mapper)
            where TEntity : class, IEntity, IAggregateRoot
            where TDestination : class
        {
            EnsureArg.IsNotNull(mapper, nameof(mapper));

            if (options == null || options?.HasIncludes() == false)
            {
                return source;
            }

            foreach (var include in (options.Includes ?? new List<IncludeOption<TEntity>>()).Insert(options.Include))
            {
                if (include.Expression != null)
                {
                    source = source.Include(
                        mapper.MapExpression<Expression<Func<TDestination, object>>>(include.Expression));
                }

                if (!include.Path.IsNullOrEmpty())
                {
                    source = source.Include(include.Path);
                }
            }

            return source;
        }
    }
}
