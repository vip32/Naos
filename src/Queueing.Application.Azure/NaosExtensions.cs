namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;
    using Naos.Queueing.Application;
    using Naos.Queueing.Domain;
    using Naos.Queueing.Infrastructure.Azure;
    using Naos.Tracing.Domain;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static QueueingOptions UseServiceBusQueue<TData>(
            this QueueingOptions options,
            Action<QueueingProviderOptions<TData>> optionsAction = null,
            TimeSpan? expiration = null,
            int? retries = null,
            string section = "naos:queueing:serviceBus")
            where TData : class
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var queueName = typeof(TData).PrettyName().ToLower();
            var configuration = options.Context.Configuration.GetSection(section).Get<ServiceBusConfiguration>();
            if (configuration?.Enabled == true)
            {
                options.Context.Services.AddScoped<IQueue<TData>>(sp =>
                {
                    return new AzureServiceBusQueue<TData>(o => o
                        .Mediator(sp.GetService<IMediator>())
                        .Tracer(sp.GetService<ITracer>())
                        .LoggerFactory(sp.GetService<ILoggerFactory>())
                        .ConnectionString(configuration.ConnectionString)
                        .QueueName(queueName)
                        .Expiration(expiration)
                        .Retries(retries));
                });

                optionsAction?.Invoke(
                    new QueueingProviderOptions<TData>(options.Context));

                options.Context.Services.AddHealthChecks()
                    .AddAzureServiceBusQueue(configuration.ConnectionString, queueName, $"queueing-provider-servicebus-{queueName}");

                options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: queueing provider added (provider={nameof(AzureServiceBusQueue<TData>)}, queue={queueName})");
            }
            else
            {
                throw new NaosException("no messaging servicebus is enabled");
            }

            return options;
        }

        public static QueueingOptions UseAzureStorageQueue<TData>(
            this QueueingOptions options,
            Action<QueueingProviderOptions<TData>> optionsAction = null,
            TimeSpan? expiration = null,
            int? retries = null,
            string section = "naos:queueing:azureStorageQueue")
            where TData : class
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var queueName = typeof(TData).PrettyName().ToLower();
            var configuration = options.Context.Configuration.GetSection(section).Get<QueueStorageConfiguration>();
            if (configuration?.Enabled == true)
            {
                options.Context.Services.AddScoped<IQueue<TData>>(sp =>
                {
                    return new AzureStorageQueue<TData>(o => o
                        .Mediator(sp.GetService<IMediator>())
                        .Tracer(sp.GetService<ITracer>())
                        .LoggerFactory(sp.GetService<ILoggerFactory>())
                        .ConnectionString(configuration.ConnectionString)
                        .QueueName(queueName)
                        .Expiration(expiration)
                        .Retries(retries));
                });

                optionsAction?.Invoke(
                    new QueueingProviderOptions<TData>(options.Context));

                options.Context.Services.AddHealthChecks()
                    .AddAzureQueueStorage(configuration.ConnectionString, queueName, $"queueing-provider-azurequeuestorage-{queueName}");

                options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: queueing provider added (provider={nameof(AzureStorageQueue<TData>)}, queue={queueName})");
            }
            else
            {
                throw new NaosException("no messaging servicebus is enabled");
            }

            return options;
        }
    }
}
