//namespace Microsoft.AspNetCore.Builder
//{
//    using System;
//    using System.Linq;
//    using Consul;
//    using EnsureThat;
//    using Microsoft.AspNetCore.Hosting;
//    using Microsoft.AspNetCore.Hosting.Server.Features;
//    using Microsoft.AspNetCore.Http.Features;
//    using Microsoft.Extensions.Configuration;
//    using Microsoft.Extensions.DependencyInjection;
//    using Naos.Foundation;
//    using Naos.ServiceDiscovery.Application;

//    /// <summary>
//    /// Extension methods for the correlation middleware.
//    /// </summary>
//    public static class ApplicationExtensions
//    {
//        /// <summary>
//        /// Enables correlation/request ids for the API request/responses.
//        /// </summary>
//        /// <param name="app"></param>
//        /// <param name="configuration"></param>
//        /// <param name="lifetime"></param>
//        /// <param name="section"></param>
//        /// <returns></returns>
//        public static IApplicationBuilder UseNaosDiscovery(this IApplicationBuilder app, IConfiguration configuration, IApplicationLifetime lifetime, string section = "naos:serviceDiscovery")
//        {
//            EnsureArg.IsNotNull(app, nameof(app));

//            app.RegisterWithConsul(lifetime);

//            return app.UseNaosDiscovery(configuration.GetSection(section).Get<DiscoveryConfiguration>(), lifetime);
//        }

//        /// <summary>
//        /// Enables correlation/request ids for the API request/responses.
//        /// </summary>
//        /// <param name="app"></param>
//        /// <param name="lifetime"></param>
//        /// <returns></returns>
//        public static IApplicationBuilder UseNaosDiscovery(this IApplicationBuilder app, IApplicationLifetime lifetime)
//        {
//            EnsureArg.IsNotNull(app, nameof(app));

//            app.RegisterWithConsul(lifetime);

//            return app.UseNaosDiscovery(new DiscoveryConfiguration(), lifetime);
//        }
//    }
//}
