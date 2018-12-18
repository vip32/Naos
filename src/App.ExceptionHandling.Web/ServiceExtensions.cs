//namespace Naos.Core.App.Exceptions.Web
//{
//    using System;
//    using System.Collections.Generic;
//    using EnsureThat;
//    using Microsoft.Extensions.DependencyInjection;
//    using Naos.Core.Common;

//    /// <summary>
//    /// Extensions on the <see cref="IServiceCollection"/>.
//    /// </summary>
//    public static class ServiceExtensions
//    {
//        public static IServiceCollection AddNaosAppExceptionHandling(
//            this IServiceCollection services,
//            IEnumerable<Type> responseHandlers,
//            bool hideDetails = false)
//        {
//            EnsureArg.IsNotNull(services, nameof(services));

//            return services.AddNaosAppExceptionHandling(responseHandlers, new ExceptionHandlerMiddlewareOptions { HideDetails = hideDetails });
//        }

//        /// <summary>
//        /// Adds required services to support the excecption handling functionality.
//        /// </summary>
//        /// <param name="services"></param>
//        /// <param name="responseHandlers"></param>
//        /// <param name="options"></param>
//        /// <returns></returns>
//        public static IServiceCollection AddNaosAppExceptionHandling(
//        this IServiceCollection services,
//        IEnumerable<Type> responseHandlers,
//        ExceptionHandlerMiddlewareOptions options)
//        {
//            EnsureArg.IsNotNull(services, nameof(services));

//            foreach (var item in responseHandlers.NullToEmpty())
//            {
//                if (item.IsAssignableFrom(typeof(IExceptionResponseHandler)))
//                {
//                    services.AddSingleton<Type>();
//                }
//            }

//            services.AddSingleton(options ?? new ExceptionHandlerMiddlewareOptions());

//            return services;
//        }
//    }
//}
