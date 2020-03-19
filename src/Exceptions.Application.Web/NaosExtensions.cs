namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Configuration.Application;
    using Naos.Foundation;
    using Naos.ServiceExceptions.Application.Web;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        /// <summary>
        /// Adds required services to support the exception handling functionality.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="hideDetails"></param>
        /// <param name="options"></param>
        public static NaosServicesContextOptions AddServiceExceptions(
            this NaosServicesContextOptions naosOptions,
            bool hideDetails = false,
            ExceptionHandlerMiddlewareOptions options = null)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.Services
                .AddSingleton(options ?? new ExceptionHandlerMiddlewareOptions() { HideDetails = hideDetails })
                .Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                    .FromExecutingAssembly()
                    .FromApplicationDependencies(a => !a.FullName.StartsWithAny(new[] { "Microsoft", "System", "Scrutor", "Consul" }))
                    .AddClasses(classes => classes.AssignableTo(typeof(IExceptionResponseHandler)), true)
                        .AsSelfWithInterfaces()
                        .WithSingletonLifetime())

                // disable automatic modelstate validation (due to AddNaosExceptionHandling), as we validate it ourselves (app.exceptions.web) and have nicer exceptions
                .Configure<ApiBehaviorOptions>(o =>
                {
                    o.SuppressModelStateInvalidFilter = true;
                });

            naosOptions.Context.Messages.Add("naos services builder: service exceptions added");
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "ServiceExceptions" });

            return naosOptions;
        }
    }
}
