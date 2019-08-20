namespace Naos.Foundation.Domain
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;

    public class HasTenantSpecification<T> : Specification<T>
        where T : ITenantEntity
    {
        public HasTenantSpecification(string tenantId)
        {
            EnsureArg.IsNotNull(tenantId);

            this.TenantId = tenantId;
        }

        protected string TenantId { get; }

        public override Expression<Func<T, bool>> ToExpression()
        {
            return t => t.TenantId == this.TenantId;
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
