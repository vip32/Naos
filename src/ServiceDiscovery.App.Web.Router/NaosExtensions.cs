namespace Microsoft.Extensions.DependencyInjection
{
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Configuration.App;
    using Naos.Core.ServiceDiscovery.App;
    using Naos.Core.ServiceDiscovery.App.Web.Router;
    using ProxyKit;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ServiceDiscoveryOptions UseFileSystemRouterRegistry(
            this ServiceDiscoveryOptions options,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddProxy(o =>
            {
                //o.ConfigurePrimaryHttpMessageHandler(c => c.GetRequiredService<HttpClientLogHandler>());
                //o.AddHttpMessageHandler<HttpClientLogHandler>();
            });

            options.Context.Services.AddSingleton(sp => new RouterContext(
                new ServiceRegistryClient(
                    new FileSystemServiceRegistry(
                        sp.GetRequiredService<ILogger<FileSystemServiceRegistry>>(),
                        options.Context.Configuration?.GetSection($"{section}:registry:fileSystem").Get<FileSystemServiceRegistryConfiguration>()))));

            options.Context.Messages.Add($"{LogEventKeys.Startup} naos builder: service discovery router added (registry={nameof(FileSystemServiceRegistry)})");
            options.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "ServiceDiscoveryRouter", EchoUri = "api/echo/servicediscovery/router" });

            return options;
        }
    }
}
