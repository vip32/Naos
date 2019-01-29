namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.ServiceDiscovery.App;
    using Naos.Core.ServiceDiscovery.App.Web.Controllers;
    using Naos.Core.ServiceDiscovery.App.Web.Router;
    using ProxyKit;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ServiceConfigurationContext AddServiceDiscoveryRouterFilesystem(
            this ServiceConfigurationContext context,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(context, nameof(context));

            context.Services.AddProxy(o =>
            {
                //o.ConfigurePrimaryHttpMessageHandler(c => c.GetRequiredService<HttpClientLogHandler>());
                //o.AddHttpMessageHandler<HttpClientLogHandler>();
            });

            context.Services.AddSingleton(sp => new RouterContext(
                new ServiceRegistryClient(
                         new FileSystemServiceRegistry(
                            sp.GetRequiredService<ILogger<FileSystemServiceRegistry>>(),
                            context.Configuration.GetSection($"{section}:registry:fileSystem").Get<FileSystemServiceRegistryConfiguration>()))));

            return context;
        }
    }
}
