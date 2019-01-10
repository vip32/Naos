namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Humanizer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.App.Web;
    using Naos.Core.Messaging.Infrastructure.FileSystem;

    public static class ServiceRegistrations
    {
        public static IServiceCollection AddNaosMessagingFileSystem(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<IMessageBroker> setupAction = null,
            string messageScope = null,
            string section = "naos:messaging:filebased")
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services.Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                .FromExecutingAssembly()
                .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)), true));

            services.AddSingleton<Hosting.IHostedService>(sp =>
                    new MessagingHostedService(sp.GetRequiredService<ILogger<MessagingHostedService>>(), sp));

            services.AddSingleton<ISubscriptionMap, SubscriptionMap>();
            services.AddSingleton<IMessageBroker>(sp =>
            {
                var result = new FileSystemMessageBroker(
                        sp.GetRequiredService<ILogger<FileSystemMessageBroker>>(),
                        new ServiceProviderMessageHandlerFactory(sp),
                        map: sp.GetRequiredService<ISubscriptionMap>(),
                        filterScope: Environment.GetEnvironmentVariable("ASPNETCORE_ISLOCAL").ToBool()
                            ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                            : string.Empty,
                        messageScope: messageScope); // PRODUCT.CAPABILITY;

                setupAction?.Invoke(result);
                return result;
            }); // scope the messagebus messages to the local machine, so local events are handled locally

            return services;
        }
    }
}
