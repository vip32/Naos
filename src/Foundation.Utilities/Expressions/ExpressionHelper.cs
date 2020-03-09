namespace Naos.Foundation
{
    using System;
    using System.Linq.Dynamic.Core;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class ExpressionHelper
    {
        public static Expression Unwrap(Expression expression)
        {
            if (expression is LambdaExpression lambdaExpression)
            {
                return lambdaExpression.Body;
            }

            return expression;
        }

        public static string GetPropertyName(Expression expression)
        {
            expression = Unwrap(expression);

            if (expression is BinaryExpression binaryExpression)
            {
                return GetPropertyName(binaryExpression.Left);
            }

            if (expression is LambdaExpression lambdaExpression)
            {
                return GetPropertyName(lambdaExpression.Body);
            }

            if (expression is UnaryExpression unaryExpression)
            {
                expression = unaryExpression.Operand;
            }

            if (expression is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            return null;
        }

        public static PropertyInfo GetProperty(Expression expression)
        {
            expression = Unwrap(expression);

            if (expression is BinaryExpression binaryExpression)
            {
                return GetProperty(binaryExpression.Left);
            }

            if (expression is LambdaExpression lambdaExpression)
            {
                return GetProperty(lambdaExpression.Body);
            }

            if (expression is UnaryExpression unaryExpression)
            {
                expression = unaryExpression.Operand;
            }

            if (expression is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
            {
                return propertyInfo;
            }

            throw new NotSupportedException(
                $"The left hand side of the expression must be a property accessor (PropertyExpression or UnaryExpression). It is a {expression.GetType()}.");
        }

        public static object GetValue(Expression expression, Type resultType)
        {
            if (expression == null)
            {
                return null;
            }

            object result;

            if (expression is ConstantExpression constantExpression)
            {
                result = constantExpression.Value;
            }
            else
            {
                var objectMember = Expression.Convert(expression, typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                result = getterLambda.Compile()();
            }

            if (resultType.GetTypeInfo().IsEnum
                && result?.GetType().GetTypeInfo().IsPrimitive == true)
            {
                return Enum.ToObject(resultType, result);
            }

            return result;
        }

        /// <summary>
        /// Cretes expression for specific property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Name of the property.</param>
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
        public static Func<T, object> GetFunc<T>(string propertyName)
        {
            return GetExpression<T>(propertyName).Compile();
        }

        public static string ToExpressionString<T>(this Expression<Func<T, bool>> source)
        {
            if (source != null)
            {
                var result = source.ToString();
                var name = result.SliceTill(" =>"); // strip the parameter from the expression
                return result.Replace($"{name}.", string.Empty).SliceFrom("=> ");
            }

            return null;
        }

        public static string ToExpressionString<T>(this Expression<Func<T, string>> source)
        {
            if (source != null)
            {
                var result = source.ToString();
                var name = result.SliceTill(" =>"); // strip the parameter from the expression
                return result.Replace($"{name}.", string.Empty).SliceFrom("=> ");
            }

            return null;
        }

        public static string ToExpressionString<T>(this Expression<Func<T, double>> source)
        {
            if (source != null)
            {
                var result = source.ToString();
                var name = result.SliceTill(" =>"); // strip the parameter from the expression
                return result.Replace($"{name}.", string.Empty).SliceFrom("=> ");
            }

            return null;
        }

        public static string ToExpressionString<T>(this Expression<Func<T, object>> source)
        {
            if (source != null)
            {
                var result = source.ToString();
                var name = result.SliceTill(" =>"); // strip the parameter from the expression
                return result.Replace($"{name}.", string.Empty).SliceFrom("=> ");
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
