namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using EnsureThat;
    using Humanizer;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;
    using Naos.Core.FileStorage.Domain;
    using Naos.Core.FileStorage.Infrastructure;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.App;
    using Naos.Core.Messaging.Infrastructure;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static MessagingOptions UseFileSystemBroker(
            this MessagingOptions options,
            Action<IMessageBroker> setupAction = null,
            string messageScope = null,
            string section = "naos:messaging:fileSystem")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<IMessageBroker>(sp =>
            {
                var configuration = options.Context.Configuration.GetSection(section).Get<FileSystemConfiguration>();
                configuration.Folder = configuration.Folder.EmptyToNull() ?? Path.GetTempPath();
                var broker = new FileSystemMessageBroker(o => o
                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                    .Mediator((IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator)))
                    .HandlerFactory(new ServiceProviderMessageHandlerFactory(sp))
                    .Storage(new FileStorageLoggingDecorator(
                        sp.GetRequiredService<ILoggerFactory>(),
                        new FolderFileStorage(s => s
                            .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                            .Folder(configuration.Folder)
                            .Serializer(new JsonNetSerializer()))))
                    .Configuration(configuration)
                    .Map(sp.GetRequiredService<ISubscriptionMap>())
                    .FilterScope(Environment.GetEnvironmentVariable(EnvironmentKeys.IsLocal).ToBool()
                            ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                            : string.Empty)
                    .MessageScope(messageScope));

                setupAction?.Invoke(broker);
                return broker;
            });

            options.Context.Messages.Add($"{LogEventKeys.Startup} naos builder: messaging added (broker={nameof(FileSystemMessageBroker)})");

            return options;
        }
    }
}
