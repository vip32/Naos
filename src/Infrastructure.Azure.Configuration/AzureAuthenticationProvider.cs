namespace Naos.Core.Operations.Infrastructure.Azure.LogAnalytics
{
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    public static class AzureAuthenticationProvider
    {
        public static async Task<AuthenticationResult> GetTokenAsync(string tenantId, string clientId, string clientSecret, string resource = "https://management.azure.com")
        {
            var authority = $"https://login.microsoftonline.com/{tenantId}";
            var authContext = new AuthenticationContext(authority, false);
            var credentials = new ClientCredential(clientId, clientSecret);
            return await authContext.AcquireTokenAsync(resource, credentials).ConfigureAwait(false);
        }
    }
}
