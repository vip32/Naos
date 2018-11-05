namespace Naos.Core.Domain.Specifications
{
    using System;
    using System.Linq;
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

#pragma warning disable SA1402 // File may only contain a single class
    public class HasTenantSpecification2 : Specification<ITenantEntity> // ITenantEntity
    //where T : class
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

        //public static class Factory
        //{
        //    public static ISpecification<T> Create(string tenantId)
        //    {
        //        return new HasTenantSpecification2<T>(tenantId);
        //    }
        //}
    }
}
