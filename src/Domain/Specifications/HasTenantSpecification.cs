namespace Naos.Core.Domain
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;

    public class HasTenantSpecification<T> : Specification<T>
        where T : ITenantEntity
    {
        protected readonly string tenantId;

        public HasTenantSpecification(string tenantId)
        {
            EnsureArg.IsNotNull(tenantId);

            this.tenantId = tenantId;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            return t => t.TenantId == this.tenantId;
        }

        public static class Factory
        {
            public static HasTenantSpecification<T> Create(string tenantId)
            {
                return new HasTenantSpecification<T>(tenantId);
            }
        }
    }
}
