namespace Naos.Core.App.Exceptions.Web
{
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Common;
    using SimpleInjector;

    public static class ServiceRegistrations
    {
        public static Container AddNaosExceptionHandling(
            this Container container,
            IServiceCollection services,
            bool hideDetails = false,
            IEnumerable<Assembly> assemblies = null)
        {
            return container.AddNaosExceptionHandling(services, new ExceptionHandlerMiddlewareOptions { HideDetails = hideDetails }, assemblies);
        }

        public static Container AddNaosExceptionHandling(
            this Container container,
             IServiceCollection services,
            ExceptionHandlerMiddlewareOptions options,
            IEnumerable<Assembly> assemblies = null)
        {
            var allAssemblies = new List<Assembly> { typeof(IExceptionResponseHandler).GetTypeInfo().Assembly };
            if (!assemblies.IsNullOrEmpty())
            {
                allAssemblies.AddRange(assemblies);
            }

            container.RegisterInstance(options ?? new ExceptionHandlerMiddlewareOptions());
            container.Collection.Register<IExceptionResponseHandler>(allAssemblies.DistinctBy(a => a.FullName)); // register all exception response handlers

            // TODO needed to disable automatic modelstate validation (due to AddNaosExceptionHandling), as we validate it ourselves (app.exceptions.web) and have nicer exceptions
            services.Configure<ApiBehaviorOptions>(o =>
            {
                o.SuppressModelStateInvalidFilter = true;
            });

            return container;
        }
    }
}
