namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Naos.Core.Operations.Domain.Repositories;
    using Naos.Core.Operations.Infrastructure.Azure.LogAnalytics;
    using Naos.Core.Operations.Infrastructure.Azure.LogAnalytics.Repositories;

    public static class ServiceExtensions
    {
        public static IServiceCollection AddNaosOperationsLogAnalytics(
            this IServiceCollection services,
            IConfiguration configuration,
            string section = "naos:operations:azureLogAnalytics")
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services.AddSingleton<ILogEventRepository>(sp =>
            {
                var logAnalyticsConfiguration = configuration.GetSection(section).Get<LogAnalyticsConfiguration>();

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

            return services;
        }
    }
}
