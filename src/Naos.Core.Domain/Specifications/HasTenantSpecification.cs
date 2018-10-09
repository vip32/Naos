namespace Naos.Core.Domain
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;

    public class HasTenantSpecification<T, TId> : Specification<T>
        where T : TenantEntity<TId>
    {
        protected readonly string tenantId;

        public HasTenantSpecification(string tenantId)
        {
            EnsureArg.IsNotNull(tenantId);

            this.tenantId = tenantId;
        }

        public override Expression<Func<T, bool>> Expression()
        {
            return t => t.TenantId == this.tenantId;
        }

        public static class Factory
        {
            public static HasTenantSpecification<T, TId> Create(string tenantId)
            {
                return new HasTenantSpecification<T, TId>(tenantId);
            }
        }
    }
}
