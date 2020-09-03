namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Foundation.Infrastructure;

    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the <seealso cref="DbContextFactory{TContext}"/> with DI.
        /// </summary>
        /// <typeparam name="TContext">The instance of <see cref="DbContext"/> to register.</typeparam>
        /// <param name="services">The instance of the <see cref="IServiceCollection"/>.</param>
        /// <param name="options">Optional access to the <see cref="DbContextOptions{TContext}"/>.</param>
        /// <param name="contextAndOptionsLifetime">Set the <see cref="ServiceLifetime"/> of the factory and options.</param>
        /// <returns>The registered <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddDbContextFactory<TContext>(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> options = null,
            ServiceLifetime contextAndOptionsLifetime = ServiceLifetime.Singleton)
            where TContext : DbContext
        {
            // instantiate with the correctly scoped provider
            services.Add(new ServiceDescriptor(
                typeof(IDbContextFactory<TContext>),
                sp => new DbContextFactory<TContext>(sp),
                contextAndOptionsLifetime));

            // dynamically run the builder on each request
            services.Add(new ServiceDescriptor(
                typeof(DbContextOptions<TContext>),
                sp => GetOptions<TContext>(options, sp),
                contextAndOptionsLifetime));

            return services;
        }

        /// <summary>
        /// Gets the options for a specific <seealso cref="TContext"/>.
        /// </summary>
        /// <param name="options">Option configuration action.</param>
        /// <param name="provider">The scoped <see cref="IServiceProvider"/>.</param>
        /// <returns>The newly configured <see cref="DbContextOptions{TContext}"/>.</returns>
        private static DbContextOptions<TContext> GetOptions<TContext>(
            Action<DbContextOptionsBuilder> options,
            IServiceProvider provider = null)
            where TContext : DbContext
        {
            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            if (provider != null)
            {
                optionsBuilder.UseApplicationServiceProvider(provider);
            }

            options?.Invoke(optionsBuilder);
            return optionsBuilder.Options;
        }
    }
}
