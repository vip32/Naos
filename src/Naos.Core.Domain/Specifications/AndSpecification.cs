namespace Naos.Core.Domain
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;

    public class AndSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> leftSpecification;
        private readonly ISpecification<T> rightSpecification;

        public AndSpecification(ISpecification<T> leftSpecification, ISpecification<T> rightSpecification)
        {
            EnsureArg.IsNotNull(leftSpecification);
            EnsureArg.IsNotNull(rightSpecification);

            this.rightSpecification = rightSpecification;
            this.leftSpecification = leftSpecification;
        }

        public override Expression<Func<T, bool>> Expression()
        {
            Expression<Func<T, bool>> leftExpression = this.leftSpecification.Expression();
            Expression<Func<T, bool>> rightExpression = this.rightSpecification.Expression();

            // BinaryExpression andExpression = Expression.AndAlso(leftExpression.Body, rightExpression.Body);
            BinaryExpression andExpression = System.Linq.Expressions.Expression.AndAlso(
                leftExpression.Body,
                System.Linq.Expressions.Expression.Invoke(rightExpression, leftExpression.Parameters[0]));

            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(andExpression, leftExpression.Parameters);
        }
    }
}
