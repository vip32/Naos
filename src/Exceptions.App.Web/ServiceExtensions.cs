namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Naos.Core.Commands.Exceptions.Web;
    using Naos.Core.Common;

    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds required services to support the exception handling functionality.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="hideDetails"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static NaosOptions AddServiceExceptions(
            this NaosOptions naosOptions,
            bool hideDetails = false,
            ExceptionHandlerMiddlewareOptions options = null)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.Services
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

            naosOptions.Context.Messages.Add($"{LogEventKeys.Startup} naos builder: service exceptions added");

            return naosOptions;
        }
    }
}
