namespace Naos.Core.Operations.Infrastructure.Azure.LogAnalytics
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
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

                var token = new AuthenticationContext(
                    $"https://login.microsoftonline.com/{authenticationConfiguration.ApiAuthentication?.TenantId}", false)
                    .AcquireTokenAsync(
                        authenticationConfiguration.ApiAuthentication?.Resource ?? "https://management.azure.com",
                        new ClientCredential(
                            authenticationConfiguration.ApiAuthentication?.ClientId,
                            authenticationConfiguration.ApiAuthentication?.ClientSecret)).Result;

                return new LogEventRepository(token?.AccessToken);
            }/*, Lifestyle.Scoped*/);

            return container;
        }
    }
}
