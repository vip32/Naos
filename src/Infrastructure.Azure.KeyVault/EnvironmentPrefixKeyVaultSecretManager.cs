namespace Naos.Core.Infrastructure.Azure.KeyVault
{
    using System;

    public class EnvironmentPrefixKeyVaultSecretManager : PrefixKeyVaultSecretManager
    {
        public EnvironmentPrefixKeyVaultSecretManager()
            : base(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() ?? "production")
        {
        }
    }
}
