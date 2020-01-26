namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Tracing.Application;
    using Naos.Tracing.Domain;
    using Naos.Tracing.Infrastructure;

    [ExcludeFromCodeCoverage]
    public static class OperationsTracingOptionsExtensions
    {
        public static OperationsTracingOptions UseZipkinExporter(
            this OperationsTracingOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var configuration = options.Context.Configuration?.GetSection("naos:operations:tracing:zipkin").Get<ZipkinSpanExporterConfiguration>();
            if (configuration != null && configuration.Enabled)
            {
                options.Context.Services.AddSingleton<ISpanExporter>(sp =>
                new ZipkinSpanExporter(
                    sp.GetRequiredService<ILogger<ZipkinSpanExporter>>(),
                    sp.GetRequiredService<Naos.Foundation.ServiceDescriptor>(),
                    configuration,
                    sp.GetRequiredService<HttpClient>())); // TODO: factory?

                if (!configuration.Endpoint.IsNullOrEmpty())
                {
                    options.Context.Services.AddHealthChecks()
                        .AddUrlGroup(new Uri(configuration.Endpoint), "tracing-exporter-zipkin", tags: new[] { "naos" });
                }

                options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: tracing exporter used (type={typeof(ZipkinSpanExporter).Name})");
            }

            // health endpoint

            return options;
        }
    }
}
