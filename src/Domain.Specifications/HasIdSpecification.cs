namespace Naos.Core.Domain.Specifications
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;

    public class HasIdSpecification<TEntity> : Specification<TEntity>
        where TEntity : IEntity
    {
        protected readonly object id;

        public HasIdSpecification(object id)
        {
            EnsureArg.IsNotNull(id);

            this.id = id;
        }

        public override Expression<Func<TEntity, bool>> ToExpression()
        {
            return t => t.Id == this.id;
        }

        public static class Factory
        {
            public static HasIdSpecification<TEntity> Create(object id)
            {
                return new HasIdSpecification<TEntity>(id);
            }
        }
    }
}
