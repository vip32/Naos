namespace Naos.Core.Domain.Specifications
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

        public override Expression<Func<TEntity, bool>> ToExpression()
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

#pragma warning disable SA1402 // File may only contain a single class
    public class HasTenantSpecification2 : Specification<ITenantEntity>
#pragma warning restore SA1402 // File may only contain a single class
    {
        protected readonly string tenantId;

        public HasTenantSpecification2(string tenantId)
        {
            EnsureArg.IsNotNull(tenantId);

            this.tenantId = tenantId;
        }

        public override Expression<Func<ITenantEntity, bool>> ToExpression()
        {
            return t => t.TenantId == this.tenantId;
        }

        public static class Factory
        {
            public static HasTenantSpecification<ITenantEntity> Create(string tenantId)
            {
                return new HasTenantSpecification<ITenantEntity>(tenantId);
            }
        }
    }
}
