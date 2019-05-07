namespace Naos.Core.Common
{
    using System;
    using System.Linq.Dynamic.Core;
    using System.Linq.Expressions;

    public static class ExpressionHelper
    {
        /// <summary>
        /// Cretes expression for specific property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static Expression<Func<T, object>> GetExpression<T>(string propertyName)
        {
            var param = Expression.Parameter(typeof(T), "t");
            Expression conversion = Expression.Convert(Expression.Property(param, propertyName), typeof(object));
            return Expression.Lambda<Func<T, object>>(conversion, param);
        }

        /// <summary>
        /// Creates delegate for specific property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static Func<T, object> GetFunc<T>(string propertyName)
        {
            return GetExpression<T>(propertyName).Compile();
        }

        public static string ToExpressionString<T>(this Expression<Func<T, bool>> source)
        {
            if(source != null)
            {
                var result = source.ToString();
                // strip the parameter from the expression
                var name = result.SubstringTill(" =>");
                return result.Replace($"{name}.", string.Empty).SubstringFrom("=> ");
            }

            return null;
        }

        public static string ToExpressionString<T>(this Expression<Func<T, object>> source)
        {
            if(source != null)
            {
                var result = source.ToString();
                // strip the parameter from the expression
                var name = result.SubstringTill(" =>");
                return result.Replace($"{name}.", string.Empty).SubstringFrom("=> ");
            }

            return null;
        }

        public static Expression<Func<T, bool>> FromExpressionString<T>(string expression)
        {
            // Param_0 = T
            return DynamicExpressionParser
               .ParseLambda<T, bool>(ParsingConfig.Default, false, expression);
        }
    }
}
