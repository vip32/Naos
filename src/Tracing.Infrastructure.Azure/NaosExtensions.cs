﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Naos.Foundation.Infrastructure;
    using Naos.Tracing.Domain;
    using Naos.Tracing.Infrastructure.Azure;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static INaosBuilderContext AddAzureLogAnalyticsTracing(this INaosBuilderContext context, string logName)
        {
            EnsureArg.IsNotNull(context, nameof(context));
            EnsureArg.IsNotNull(context.Services, nameof(context.Services));

            var configuration = context.Configuration?.GetSection("naos:operations:tracing:azureLogAnalytics").Get<LogAnalyticsConfiguration>(); // TODO: move to operations:tracing:azureLogAnalytics
            if (configuration != null)
            {
                context.Services.AddScoped<ILogTraceRepository>(sp =>
                {
                    // authenticate api https://dev.int.loganalytics.io/documentation/1-Tutorials/ARM-API
                    var token = new AuthenticationContext(
                            $"https://login.microsoftonline.com/{configuration.ApiAuthentication?.TenantId}", false)
                            .AcquireTokenAsync(
                                configuration.ApiAuthentication?.Resource ?? "https://management.azure.com",
                                new ClientCredential(
                                    configuration.ApiAuthentication?.ClientId,
                                    configuration.ApiAuthentication?.ClientSecret)).Result;

                    configuration.LogName ??= $"{logName.Replace("_CL", string.Empty)}_CL";
                    return new LogAnalyticsLogTraceRepository(
                        sp.GetRequiredService<ILoggerFactory>(),
                        new System.Net.Http.HttpClient(), // TODO: resolve from container!
                        configuration,
                        token?.AccessToken);
                });
                context.Messages.Add($"naos services builder: logging azure loganalytics repository added (name={logName}_CL, workspace={configuration.WorkspaceId})");
            }

            return context;
        }
    }
}
