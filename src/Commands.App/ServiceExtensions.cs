namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Naos.Core.Commands.App;

    public static class ServiceExtensions
    {
        public static IServiceCollection AddNaosAppCommands(
            this IServiceCollection services)
        {
            return services
                .Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                    .FromExecutingAssembly()
                    .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandBehavior)), true));
        }
    }
}
