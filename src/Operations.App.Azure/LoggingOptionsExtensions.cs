namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using global::Serilog;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure;
    using Naos.Core.Operations.App;
    using Naos.Core.Operations.Domain.Repositories;
    using Naos.Core.Operations.Infrastructure.Azure;

    public static class LoggingOptionsExtensions
    {
        public static LoggingOptions AddAzureApplicationInsights(this LoggingOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var appInsightsConfiguration = options.Context.Configuration?.GetSection("naos:operations:logEvents:azureApplicationInsights").Get<ApplicationInsightsConfiguration>();
            if (appInsightsConfiguration?.Enabled == true
                && appInsightsConfiguration?.ApplicationKey.IsNullOrEmpty() == false)
            {
                // configure the serilog sink
                //options.LoggerConfiguration.WriteTo.AppInsights(appInsightsConfiguration.ApplicationKey);

                options.Messages.Add($"{LogEventKeys.Operations} logging: azureapplicationinsightssink added (application={appInsightsConfiguration.ApplicationKey})");
            }

            return options;
        }

        public static LoggingOptions AddAzureLogAnalytics(this LoggingOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var configuration = options.Context.Configuration?.GetSection("naos:operations:azureLogAnalytics").Get<LogAnalyticsConfiguration>(); // TODO: move to operations:logevents:azureLogAnalytics

            // configure the serilog sink
            var logName = configuration?.LogName.EmptyToNull() ?? "LogEvents_[ENVIRONMENT]"
                .Replace("[ENVIRONMENT]", options.Environment)
                .Replace("[PRODUCT]", options.Context.Descriptor?.Product)
                .Replace("[CAPABILITY]", options.Context.Descriptor?.Capability);
            if (logName.IsNullOrEmpty())
            {
                return options;
            }

            if (configuration?.Enabled == true
                && configuration?.WorkspaceId.IsNullOrEmpty() == false
                && configuration?.AuthenticationId.IsNullOrEmpty() == false)
            {
                options.LoggerConfiguration.WriteTo.AzureAnalytics(
                    configuration.WorkspaceId,
                    configuration.AuthenticationId,
                    logName: logName, // without _CL
                    storeTimestampInUtc: true,
                    logBufferSize: configuration.BufferSize,
                    batchSize: configuration.BatchSize);

                options.Messages.Add($"{LogEventKeys.Operations} logging: azureloganalytics sink added (name={logName}_CL, workspace={configuration.WorkspaceId})");
            }

            // configure the repository for the dashboard (controller)
            options.Context.Services.AddScoped<ILogEventRepository>(sp =>
            {
                // authenticate api https://dev.int.loganalytics.io/documentation/1-Tutorials/ARM-API
                var token = new AuthenticationContext(
                    $"https://login.microsoftonline.com/{configuration.ApiAuthentication?.TenantId}", false)
                    .AcquireTokenAsync(
                        configuration.ApiAuthentication?.Resource ?? "https://management.azure.com",
                        new ClientCredential(
                            configuration.ApiAuthentication?.ClientId,
                            configuration.ApiAuthentication?.ClientSecret)).Result;

                return new LogAnalyticsRepository(
                    new System.Net.Http.HttpClient(), // TODO: resolve from container!
                    token?.AccessToken,
                    configuration.SubscriptionId,
                    configuration.ResourceGroupName,
                    configuration.WorkspaceName,
                    $"{logName}_CL");
            });

            return options;
        }
    }
}
