namespace Naos.Configuration.Application
{
    public struct ConfigurationKeys
    {
        public const string KeyVaultEnabled = "naos:secrets:vault:enabled";

        public const string KeyVaultName = "naos:secrets:vault:name";

        public const string KeyVaultClientId = "naos:secrets:vault:clientId";

        public const string KeyVaultClientSecret = "naos:secrets:vault:clientSecret";

        public const string AppConfigurationUrl = "naos:secrets:azureAppConfiguration:connectionString";
    }
}
