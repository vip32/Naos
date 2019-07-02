﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System.Linq;
    using System.Reflection;
    using MediatR;
    using Microsoft.Extensions.DependencyModel;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediatr(this IServiceCollection services)
        {
            var context = DependencyContext.Default;

            // find all assembly references (including unloaded), so mediatr can inspect all available assemblies
            var assemblies = context.RuntimeLibraries
                .SelectMany(l => l.GetDefaultAssemblyNames(context))
                .Where(a => !a.Name.StartsWith("Microsoft.") && !a.Name.StartsWith("System.") && !a.Name.StartsWith("Naos.Foundation"))
                .Select(Assembly.Load)
                .Where(a => !a.GlobalAssemblyCache)
                .ToArray();

            return services.AddMediatR(assemblies);
        }
    }
}
