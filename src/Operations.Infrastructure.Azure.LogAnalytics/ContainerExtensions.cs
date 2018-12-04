namespace Naos.Core.Operations.Infrastructure.Azure.LogAnalytics
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Naos.Core.Operations.Domain.Repositories;
    using Naos.Core.Operations.Infrastructure.Azure.LogAnalytics.Repositories;
    using SimpleInjector;

    public static class ContainerExtensions
    {
        public static Container AddNaosOperations(
            this Container container,
            IConfiguration configuration,
            string section = "naos:operations:azureLogAnalytics")
        {
            container.Register<ILogEventRepository>(() =>
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

            return container;
        }
    }
}
