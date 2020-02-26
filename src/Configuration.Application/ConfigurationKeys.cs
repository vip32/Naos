namespace Naos.Configuration.Application
{
    public struct ConfigurationKeys
    {
        public const string UserSecretsId = "naos:secrets:userSecretsId";

        public const string AzureKeyVaultEnabled = "naos:secrets:vault:enabled";

        public const string AzureKeyVaultName = "naos:secrets:vault:name";

        public const string AzureKeyVaultClientId = "naos:secrets:vault:clientId";

        public const string AzureKeyVaultClientSecret = "naos:secrets:vault:clientSecret";

        public const string AzureAppConfigurationEnabled = "naos:secrets:azureAppConfiguration:enabled";

        public const string AzureAppConfigurationConnectionString = "naos:secrets:azureAppConfiguration:connectionString";
    }
}
