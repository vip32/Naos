namespace Naos.Core.Operations.Infrastructure.Azure.LogAnalytics
{
    using Microsoft.Extensions.Configuration;
    using Naos.Core.Operations.Domain.Repositories;
    using Naos.Core.Operations.Infrastructure.Azure.LogAnalytics.Repositories;
    using SimpleInjector;

    public static class ContainerExtension
    {
        public static Container AddNaosOperations(
            this Container container,
            IConfiguration configuration,
            string section = "naos:operations:azureLogAnalytics")
        {
            container.Register<ILogEventRepository>(() =>
            {
                var authenticationConfiguration = configuration.GetSection(section).Get<LogAnalyticsConfiguration>();
                var token = AzureAuthenticationProvider.GetTokenAsync(
                    authenticationConfiguration.ApiAuthentication?.TenantId,
                    authenticationConfiguration.ApiAuthentication?.ClientId,
                    authenticationConfiguration.ApiAuthentication?.ClientSecret,
                    authenticationConfiguration.ApiAuthentication?.Resource ?? "https://management.azure.com").Result;

                return new LogEventRepository(token.AccessToken);
            }/*, Lifestyle.Scoped*/);

            return container;
        }
    }
}
