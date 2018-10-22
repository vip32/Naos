namespace Naos.Core.Domain.Specifications
{
    using System;
    using System.Linq.Expressions;

    public class IdentitySpecification<T> : Specification<T>
    {
        public override Expression<Func<T, bool>> ToExpression()
        {
            return x => true;
        }
    }
}
