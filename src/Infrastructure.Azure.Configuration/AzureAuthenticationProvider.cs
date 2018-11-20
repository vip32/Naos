//namespace Naos.Core.Operations.Infrastructure.Azure.LogAnalytics
//{
//    using System.Threading.Tasks;
//    using Microsoft.IdentityModel.Clients.ActiveDirectory;

//    public static class AzureAuthenticationProvider
//    {
//        public static async Task<AuthenticationResult> GetTokenAsync(string tenantId, string clientId, string clientSecret, string resource)
//        {
//            return await new AuthenticationContext($"https://login.microsoftonline.com/{tenantId}", false)
//                .AcquireTokenAsync(
//                    resource ?? "https://management.azure.com",
//                    new ClientCredential(clientId, clientSecret)).ConfigureAwait(false);
//        }
//    }
//}
