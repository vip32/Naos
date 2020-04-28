namespace Naos.Foundation.Domain
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using EnsureThat;

    public class AndSpecification : Specification
    {
        private readonly ISpecification leftSpecification;
        private readonly ISpecification rightSpecification;

        public AndSpecification(ISpecification leftSpecification, ISpecification rightSpecification)
        {
            EnsureArg.IsNotNull(leftSpecification);
            EnsureArg.IsNotNull(rightSpecification);

            this.rightSpecification = rightSpecification;
            this.leftSpecification = leftSpecification;
        }

        public override Expression<Func<bool>> ToExpression()
        {
            var leftExpression = this.leftSpecification.ToExpression();
            var rightExpression = this.rightSpecification.ToExpression();

            // mongo fix: https://stackoverflow.com/questions/54959750/mongodb-expression-system-invalidoperationexception-is-not-supported
            //var param = leftExpression.Parameters[0];
            //if (ReferenceEquals(param, rightExpression.Parameters[0]))
            //{
            //    // simple version
            //    return Expression.Lambda<Func<T, bool>>(
            //        Expression.AndAlso(leftExpression.Body, rightExpression.Body), param);
            //}

            //var andExpression = Expression.AndAlso(leftExpression.Body, rightExpression.Body);
            var andExpression = Expression.AndAlso(
                leftExpression.Body,
                Expression.Invoke(rightExpression, leftExpression.Parameters.Single()));

            //return Expression.Lambda<Func<T, bool>>(andExpression, leftExpression.Parameters.Single());
            return Expression.Lambda<Func<bool>>(andExpression, leftExpression.Parameters);
        }

        public override string ToString()
        {
            return this.ToExpression()?.ToString();
        }
    }
}
