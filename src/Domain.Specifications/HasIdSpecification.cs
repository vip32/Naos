namespace Naos.Foundation.Domain
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;

    public class HasIdSpecification<T> : Specification<T>
        where T : IEntity
    {
        protected readonly object id;

        public HasIdSpecification(object id)
        {
            EnsureArg.IsNotNull(id);

            this.id = id;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            return t => t.Id == this.id;
        }

        public static class Factory
        {
            public static HasIdSpecification<T> Create(object id)
            {
                return new HasIdSpecification<T>(id);
            }
        }
    }
}
