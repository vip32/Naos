namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Linq.Expressions;

    public static partial class Extensions
    {
        /// <summary>
        /// Converts the Get expression to a Set expression
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="source">The source expression.</param>
        /// <returns></returns>
        public static Action<TEntity, string> ToSetAction<TEntity>(this Expression<Func<TEntity, string>> source)
        {
            if (source == null)
            {
                return null;
            }

            var member = (MemberExpression)source.Body;
            var param = Expression.Parameter(typeof(string), "value");
            var set = Expression.Lambda<Action<TEntity, string>>(
                Expression.Assign(member, param), source.Parameters[0], param);
            return set.Compile();
        }

        /// <summary>
        /// Converts the Get expression to a Set expression
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="source">The source expression.</param>
        /// <returns></returns>
        public static Action<TEntity, int> ToSetAction<TEntity>(this Expression<Func<TEntity, int>> source)
        {
            if (source == null)
            {
                return null;
            }

            var member = (MemberExpression)source.Body;
            var param = Expression.Parameter(typeof(int), "value");
            var set = Expression.Lambda<Action<TEntity, int>>(
                Expression.Assign(member, param), source.Parameters[0], param);
            return set.Compile();
        }

        /// <summary>
        /// Converts the Get expression to a Set expression
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="source">The source expression.</param>
        /// <returns></returns>
        public static Action<TEntity, DateTime?> ToSetAction<TEntity>(this Expression<Func<TEntity, DateTime?>> source)
        {
            if (source == null)
            {
                return null;
            }

            var member = (MemberExpression)source.Body;
            var param = Expression.Parameter(typeof(DateTime?), "value");
            var set = Expression.Lambda<Action<TEntity, DateTime?>>(
                Expression.Assign(member, param), source.Parameters[0], param);
            return set.Compile();
        }
    }
}
