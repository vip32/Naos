namespace Naos.Core.Configuration.App
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.Rest.Azure;

    public class CachedKeyVaultClient : KeyVaultClient // TODO: move to Infrastructure.Azure
    {
        public CachedKeyVaultClient(AuthenticationCallback authenticationCallback, params System.Net.Http.DelegatingHandler[] handlers)
            : base(authenticationCallback, handlers)
        {
        }

        public CachedKeyVaultClient(AuthenticationCallback authenticationCallback, System.Net.Http.HttpClient httpClient)
            : base(authenticationCallback, httpClient)
        {
        }

        public CachedKeyVaultClient(KeyVaultCredential credential, System.Net.Http.HttpClient httpClient)
            : base(credential, httpClient)
        {
        }

        public CachedKeyVaultClient(Microsoft.Rest.ServiceClientCredentials credentials, params System.Net.Http.DelegatingHandler[] handlers)
            : base(credentials, handlers)
        {
        }

        public CachedKeyVaultClient(Microsoft.Rest.ServiceClientCredentials credentials, System.Net.Http.HttpClientHandler rootHandler, params System.Net.Http.DelegatingHandler[] handlers)
            : base(credentials, rootHandler, handlers)
        {
        }

        protected CachedKeyVaultClient(params System.Net.Http.DelegatingHandler[] handlers)
            : base(handlers)
        {
        }

        protected CachedKeyVaultClient(System.Net.Http.HttpClientHandler rootHandler, params System.Net.Http.DelegatingHandler[] handlers)
            : base(rootHandler, handlers)
        {
        }

        public new Task<AzureOperationResponse<IPage<SecretItem>>> GetSecretsWithHttpMessagesAsync(
            string vaultBaseUrl,
            int? maxresults = null,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public new Task<AzureOperationResponse<SecretBundle>> GetSecretWithHttpMessagesAsync(
            string vaultBaseUrl,
            string secretName,
            string secretVersion,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}