namespace Naos.Core.Infrastructure.Azure.KeyVault
{
    using System;
    using Naos.Core.Common;

    public class EnvironmentPrefixKeyVaultSecretManager : PrefixKeyVaultSecretManager
    {
        public EnvironmentPrefixKeyVaultSecretManager()
            : base(Environment.GetEnvironmentVariable(EnvironmentKeys.Environment)?.ToLower() ?? "production")
        {
        }
    }
}
