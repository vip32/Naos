namespace Naos.Foundation.Domain
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;

    public class HasIdSpecification<T> : Specification<T>
        where T : IEntity
    {
        public HasIdSpecification(object id)
        {
            EnsureArg.IsNotNull(id);

            this.Id = id;
        }

        protected object Id { get; }

        public override Expression<Func<T, bool>> ToExpression()
        {
            return t => t.Id == this.Id;
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
