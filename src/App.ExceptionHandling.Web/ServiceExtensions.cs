namespace Naos.Core.App.ExceptionHandling.Web
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Common;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceExtensions
    {
        public static IServiceCollection AddNaosAppExceptionHandling(
            this IServiceCollection serviceCollection,
            IEnumerable<Type> responseHandlers,
            bool hideDetails = false)
        {
            return serviceCollection.AddNaosAppExceptionHandling(responseHandlers, new ExceptionHandlerMiddlewareOptions { HideDetails = hideDetails });
        }

        /// <summary>
        /// Adds required services to support the excecption handling functionality.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="responseHandlers"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddNaosAppExceptionHandling(
        this IServiceCollection serviceCollection,
        IEnumerable<Type> responseHandlers,
        ExceptionHandlerMiddlewareOptions options)
        {
            foreach (var item in responseHandlers.NullToEmpty())
            {
                if (item.IsAssignableFrom(typeof(IExceptionResponseHandler)))
                {
                    serviceCollection.AddSingleton<Type>();
                }
            }

            serviceCollection.AddSingleton(options ?? new ExceptionHandlerMiddlewareOptions());

            return serviceCollection;
        }
    }
}
