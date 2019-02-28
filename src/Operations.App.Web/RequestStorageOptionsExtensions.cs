namespace Microsoft.Extensions.DependencyInjection
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;
    using Naos.Core.FileStorage.Infrastructure;
    using Naos.Core.Infrastructure.Azure;
    using Naos.Core.Operations.App;
    using Naos.Core.Operations.App.Web;

    [ExcludeFromCodeCoverage]
    public static class RequestStorageOptionsExtensions
    {
        public static RequestStorageOptions UseFolderStorage(
            this RequestStorageOptions options,
            string section = "naos:operations:logging:file")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var configuration = options.Context.Configuration?.GetSection(section).Get<LogFileConfiguration>();
            if (configuration?.Enabled == true)
            {
                options.Context.Messages.Add($"{LogEventKeys.Startup} naos services builder: request storage added (type={typeof(FolderFileStorage).Name})");
                options.Context.Services.AddSingleton(sp =>
                    new RequestStorageMiddlewareOptions
                    {
                        Storage = new FileStorageScopedDecorator("requests/{yyyy}/{MM}/{dd}",
                            new FileStorageLoggingDecorator(
                                sp.GetRequiredService<ILoggerFactory>(),
                                new FolderFileStorage(f => f.Folder(Path.Combine(Path.GetTempPath(), "naos_operations")))))
                    });
            }

            return options;
        }

        public static RequestStorageOptions UseAzureBlobStorage(
            this RequestStorageOptions options,
            string section = "naos:operations:logging:azureBlobStorage")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var configuration = options.Context.Configuration?.GetSection("naos:operations:logging:azureBlobStorage").Get<BlobStorageConfiguration>();
            if (configuration?.Enabled == true)
            {
                options.Context.Messages.Add($"{LogEventKeys.Startup} naos services builder: request storage added (type={typeof(AzureBlobFileStorage).Name})");
                var connectionString = options.Context.Configuration[$"{section}:connectionString"];
                options.Context.Services.AddSingleton(sp =>
                    new RequestStorageMiddlewareOptions
                    {
                        Storage = new FileStorageScopedDecorator("requests/{yyyy}/{MM}/{dd}",
                            new FileStorageLoggingDecorator(
                                sp.GetRequiredService<ILoggerFactory>(),
                                new AzureBlobFileStorage(f => f
                                    .ContainerName($"{options.Context.Environment.ToLower()}-operations")
                                    .ConnectionString(connectionString))))
                    });

                options.Context.Services
                    .AddHealthChecks()
                        .AddAzureBlobStorage(connectionString, "operations-requeststorage-blobstorage");
            }

            return options;
        }
    }
}
