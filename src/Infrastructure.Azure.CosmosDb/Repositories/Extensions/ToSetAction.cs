namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Linq.Expressions;

    public static partial class Extensions
    {
        /// <summary>
        /// Converts the Get expression to a Set expression
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="source">The source expression.</param>
        /// <returns></returns>
        public static Action<T, string> ToSetAction<T>(this Expression<Func<T, string>> source)
        {
            if(source == null)
            {
                return null;
            }

            var member = (MemberExpression)source.Body;
            var param = Expression.Parameter(typeof(string), "value");
            var set = Expression.Lambda<Action<T, string>>(
                Expression.Assign(member, param), source.Parameters[0], param);
            return set.Compile(); // replace wit CompileFast()? https://github.com/dadhi/FastExpressionCompiler
        }

        /// <summary>
        /// Converts the Get expression to a Set expression
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="source">The source expression.</param>
        /// <returns></returns>
        public static Action<T, int> ToSetAction<T>(this Expression<Func<T, int>> source)
        {
            if(source == null)
            {
                return null;
            }

            var member = (MemberExpression)source.Body;
            var param = Expression.Parameter(typeof(int), "value");
            var set = Expression.Lambda<Action<T, int>>(
                Expression.Assign(member, param), source.Parameters[0], param);
            return set.Compile(); // replace wit CompileFast()? https://github.com/dadhi/FastExpressionCompiler
        }

        /// <summary>
        /// Converts the Get expression to a Set expression
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="source">The source expression.</param>
        /// <returns></returns>
        public static Action<T, DateTime?> ToSetAction<T>(this Expression<Func<T, DateTime?>> source)
        {
            if(source == null)
            {
                return null;
            }

            var member = (MemberExpression)source.Body;
            var param = Expression.Parameter(typeof(DateTime?), "value");
            var set = Expression.Lambda<Action<T, DateTime?>>(
                Expression.Assign(member, param), source.Parameters[0], param);
            return set.Compile(); // replace wit CompileFast()? https://github.com/dadhi/FastExpressionCompiler
        }
    }
}
