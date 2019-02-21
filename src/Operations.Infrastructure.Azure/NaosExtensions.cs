namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure;
    using Naos.Core.Operations.Domain.Repositories;
    using Naos.Core.Operations.Infrastructure.Azure;

    public static class NaosExtensions
    {
        public static INaosBuilderContext AddAzureLogAnalytics(this INaosBuilderContext context, string logName)
        {
            EnsureArg.IsNotNull(context, nameof(context));
            EnsureArg.IsNotNull(context.Services, nameof(context.Services));

            var configuration = context.Configuration?.GetSection("naos:operations:logging:azureLogAnalytics").Get<LogAnalyticsConfiguration>(); // TODO: move to operations:logevents:azureLogAnalytics
            if (configuration != null)
            {
                context.Services.AddScoped<ILogEventRepository>(sp =>
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
                        sp.GetRequiredService<ILoggerFactory>(),
                        new System.Net.Http.HttpClient(), // TODO: resolve from container!
                        token?.AccessToken,
                        configuration.SubscriptionId,
                        configuration.ResourceGroupName,
                        configuration.WorkspaceName,
                        $"{logName.Replace("_CL", string.Empty)}_CL");
                });
                context.Messages.Add($"{LogEventKeys.Startup} logging: azure loganalytics repository added (name={logName}_CL, workspace={configuration.WorkspaceId})");
            }

            return context;
        }
    }
}
