namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Commands.Application.Web;
    using Naos.FileStorage.Domain;
    using Naos.FileStorage.Infrastructure;
    using Naos.Foundation;
    using Naos.Queueing.Domain;
    using Naos.Queueing.Infrastructure.Azure;
    using Naos.Tracing.Domain;

    [ExcludeFromCodeCoverage]
    public static class CommandRequestOptionsExtensions
    {
        public static CommandRequestOptions UseAzureBlobStorage(
            this CommandRequestOptions options, string connectionString = null, string containerName = "commandrequests")
        {
            options.Context.Services.AddSingleton(sp => new CommandRequestStore(
                new FileStorageLoggingDecorator(
                    sp.GetRequiredService<ILoggerFactory>(),
                    new AzureBlobFileStorage(o => o
                        .ConnectionString(connectionString.EmptyToNull() ?? options.Context.Configuration["naos:commands:azureBlobStorage:connectionString"])
                        .ContainerName($"{containerName}-{HashAlgorithm.ComputeMd5Hash(options.Context.Descriptor.Name)}")))));

            return options;
        }

        public static CommandRequestOptions UseAzureBlobStorage(
            this CommandRequestOptions options, Builder<AzureBlobFileStorageOptionsBuilder, AzureBlobFileStorageOptions> optionsBuilder)
        {
            options.Context.Services.AddSingleton(sp => new CommandRequestStore(
                new FileStorageLoggingDecorator(
                    sp.GetRequiredService<ILoggerFactory>(),
                    new AzureBlobFileStorage(optionsBuilder))));

            return options;
        }

        public static CommandRequestOptions UseAzureStorageQueue(
            this CommandRequestOptions options, string connectionString = null, string name = "commandrequests")
        {
            options.Context.Services.AddSingleton<IQueue<CommandRequestWrapper>>(sp =>
                new AzureStorageQueue<CommandRequestWrapper>(o => o
                        .Mediator(sp.GetRequiredService<IMediator>())
                        .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                        .ConnectionString(connectionString.EmptyToNull() ?? options.Context.Configuration["naos:commands:azureStorageQueue:connectionString"])
                        .Serializer(new JsonNetSerializer(TypedJsonSerializerSettings.Create())) // needs type information in json to deserialize correctly (which is needed for mediator.send)
                        .QueueName($"{name}-{HashAlgorithm.ComputeMd5Hash(options.Context.Descriptor.Name)}")));

            return options;
        }

        public static CommandRequestOptions UseAzureStorageQueue(
            this CommandRequestOptions options, Builder<AzureStorageQueueOptionsBuilder, AzureStorageQueueOptions> optionsBuilder)
        {
            options.Context.Services.AddSingleton<IQueue<CommandRequestWrapper>>(sp =>
                new AzureStorageQueue<CommandRequestWrapper>(optionsBuilder));

            return options;
        }

        public static CommandRequestOptions UseAzureServiceBusQueue(
            this CommandRequestOptions options,
            string connectionString = null,
            string name = "commandrequests",
            TimeSpan? expiration = null,
            int? retries = null)
        {
            options.Context.Services.AddSingleton<IQueue<CommandRequestWrapper>>(sp =>
                new AzureServiceBusQueue<CommandRequestWrapper>(o => o
                        .Mediator(sp.GetRequiredService<IMediator>())
                        .Tracer(sp.GetService<ITracer>())
                        .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                        .ConnectionString(connectionString.EmptyToNull() ?? options.Context.Configuration["naos:commands:azureServiceBusQueue:connectionString"])
                        .Serializer(new JsonNetSerializer(TypedJsonSerializerSettings.Create())) // needs type information in json to deserialize correctly (which is needed for mediator.send)
                        .QueueName($"{name}-{HashAlgorithm.ComputeMd5Hash(options.Context.Descriptor.Name)}")
                        .Expiration(expiration)
                        .Retries(retries)));

            return options;
        }
    }
}
