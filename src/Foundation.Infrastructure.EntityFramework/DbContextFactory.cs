namespace Naos.Foundation.Infrastructure
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public class DbContextFactory<TContext> : IDbContextFactory<TContext>
        where TContext : DbContext
    {
        private readonly IServiceProvider provider;

        public DbContextFactory(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public TContext Create()
        {
            if (this.provider == null)
            {
                throw new InvalidOperationException(
                    "An instance of IServiceProvider must be registered");
            }

            return ActivatorUtilities.CreateInstance<TContext>(this.provider);
        }
    }
}
