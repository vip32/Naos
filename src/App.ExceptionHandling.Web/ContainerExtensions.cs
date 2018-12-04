namespace Naos.Core.App.ExceptionHandling.Web
{
    using System.Collections.Generic;
    using System.Reflection;
    using Naos.Core.Common;
    using SimpleInjector;

    public static class ContainerExtensions
    {
        public static Container AddNaosExceptionHandling(
            this Container container,
            bool hideDetails = false,
            IEnumerable<Assembly> assemblies = null)
        {
            return container.AddNaosExceptionHandling(new ExceptionHandlerMiddlewareOptions { HideDetails = hideDetails }, assemblies);
        }

        public static Container AddNaosExceptionHandling(
            this Container container,
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

            return container;
        }
    }
}
