namespace Naos.Core.Domain
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;

    public class HasTenantSpecification<TEntity> : Specification<TEntity>
        where TEntity : ITenantEntity
    {
        protected readonly string tenantId;

        public HasTenantSpecification(string tenantId)
        {
            EnsureArg.IsNotNull(tenantId);

            this.tenantId = tenantId;
        }

        public override Expression<Func<TEntity, bool>> Expression()
        {
            return t => t.TenantId == this.tenantId;
        }

        public static class Factory
        {
            public static HasTenantSpecification<TEntity> Create(string tenantId)
            {
                return new HasTenantSpecification<TEntity>(tenantId);
            }
        }
    }
}
