namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Naos.Core.Commands.Exceptions.Web;

    public static class ServiceExtensions
    {
        public static IServiceCollection AddNaosServiceExceptions(
            this IServiceCollection services,
            bool hideDetails = false,
            ExceptionHandlerMiddlewareOptions options = null)
        {
            EnsureArg.IsNotNull(services, nameof(services));

            return services
                .AddSingleton(options ?? new ExceptionHandlerMiddlewareOptions() { HideDetails = hideDetails })

                .Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                    .FromExecutingAssembly()
                    .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                    .AddClasses(classes => classes.AssignableTo(typeof(IExceptionResponseHandler)), true)
                        .AsSelfWithInterfaces()
                        .WithSingletonLifetime())

                // disable automatic modelstate validation (due to AddNaosExceptionHandling), as we validate it ourselves (app.exceptions.web) and have nicer exceptions
                .Configure<ApiBehaviorOptions>(o =>
                {
                    o.SuppressModelStateInvalidFilter = true;
                });
        }
    }
}
