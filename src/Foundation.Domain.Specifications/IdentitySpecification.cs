namespace Naos.Foundation.Domain
{
    using System;
    using System.Linq.Expressions;

#pragma warning disable SA1402 // File may only contain a single type
    public class IdentitySpecification<T> : Specification<T>
#pragma warning restore SA1402 // File may only contain a single type
    {
        public override Expression<Func<T, bool>> ToExpression()
        {
            return x => true;
        }
    }

    public class IdentitySpecification : Specification
    {
        public override Expression<Func<bool>> ToExpression()
        {
            return () => true;
        }
    }
}
