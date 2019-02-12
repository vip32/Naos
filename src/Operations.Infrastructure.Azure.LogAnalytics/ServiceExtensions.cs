namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Naos.Core.Operations.App;
    using Naos.Core.Operations.Domain.Repositories;
    using Naos.Core.Operations.Infrastructure.Azure.LogAnalytics;

    public static class ServiceExtensions
    {
        public static OperationsOptions AddLogAnalyticsDashboard(
            this OperationsOptions options,
            string section = "naos:operations:azureLogAnalytics")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var logAnalyticsConfiguration = options.Context.Configuration?.GetSection(section).Get<LogAnalyticsConfiguration>();
            options.Context.Services.AddScoped<ILogEventRepository>(sp =>
            {
                // authenticate api https://dev.int.loganalytics.io/documentation/1-Tutorials/ARM-API
                var token = new AuthenticationContext(
                    $"https://login.microsoftonline.com/{logAnalyticsConfiguration.ApiAuthentication?.TenantId}", false)
                    .AcquireTokenAsync(
                        logAnalyticsConfiguration.ApiAuthentication?.Resource ?? "https://management.azure.com",
                        new ClientCredential(
                            logAnalyticsConfiguration.ApiAuthentication?.ClientId,
                            logAnalyticsConfiguration.ApiAuthentication?.ClientSecret)).Result;

                return new LogEventRepository(
                    new System.Net.Http.HttpClient(), // TODO: resolve from container!
                    token?.AccessToken,
                    logAnalyticsConfiguration.SubscriptionId,
                    logAnalyticsConfiguration.ResourceGroupName,
                    logAnalyticsConfiguration.WorkspaceName);
            }/*, Lifestyle.Scoped*/);

            return options;
        }
    }
}
