namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using System.Reflection;
    using MediatR;
    using Microsoft.Extensions.DependencyModel;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediatr(this IServiceCollection services)
        {
            if (services == null)
            {
                return services;
            }

            var context = DependencyContext.Default;

            // find all assembly references (including unloaded), so mediatr can inspect all available assemblies
            var assemblies = context.RuntimeLibraries
                .SelectMany(l => l.GetDefaultAssemblyNames(context))
                .Where(a => !a.Name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase)
                    && !a.Name.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
                    && !a.Name.StartsWith("Naos.Foundation", StringComparison.OrdinalIgnoreCase))
                .Select(Assembly.Load)
                .Where(a => !a.GlobalAssemblyCache)
                .ToArray();

            return services.AddMediatR(assemblies);
        }
    }
}
