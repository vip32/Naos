namespace Naos.Foundation.Infrastructure
{
    using System;

    public class EnvironmentPrefixKeyVaultSecretManager : PrefixKeyVaultSecretManager
    {
        public EnvironmentPrefixKeyVaultSecretManager()
            : base(Environment.GetEnvironmentVariable(EnvironmentKeys.Environment)?.ToLower() ?? "production")
        {
        }
    }
}
