namespace Naos.Core.App.Configuration
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure.KeyVault;

    public static class NaosConfigurationFactory
    {
        //public static void Bind<T>(T instance, string section, string basePath = null, string[] args = null)
        //{
        //    if (instance == null || section.IsNullOrEmpty())
        //    {
        //        return;
        //    }

        //    CreateRoot(basePath, args)
        //        .GetSection(section)
        //        .Bind(instance);
        //}

        public static IConfigurationBuilder Extend(IConfigurationBuilder config, string basePath = null, string[] args = null)
        {
            return CreateBuilder(basePath, args, config);
        }

        public static IConfigurationRoot CreateRoot(string basePath = null, string[] args = null)
        {
            return CreateBuilder(basePath, args).Build();
        }

        private static IConfigurationBuilder CreateBuilder(string basePath = null, string[] args = null, IConfigurationBuilder config = null)
        {
            config = config ?? new ConfigurationBuilder();
            config.SetBasePath(basePath ?? AppDomain.CurrentDomain.BaseDirectory)
                  .AddJsonFile("appsettings.json", optional: true)
                  .AddEnvironmentVariables();

            if (args != null)
            {
                config.AddCommandLine(args);
            }

            // env >>
            var builtConfig = config.Build();
            if (builtConfig["naos:secrets:vault:enabled"].ToBool(true))
            {
                if (builtConfig["naos:secrets:vault:name"].IsNullOrEmpty()
                    || builtConfig["naos:secrets:vault:clientId"].IsNullOrEmpty()
                    || builtConfig["naos:secrets:vault:clientSecret"].IsNullOrEmpty())
                {
                    throw new Exception("Naos Keyvault configuration provider cannot be used when secrets:keyvault:name, secrets:keyvault:clientId or secrets:keyvault:clientSecret are not provided by any of the configuration providers (json/env/args). Please make these configuration settings available or set secrets:keyvault:enabled to 'false'.");
                }

                config.AddAzureKeyVault(
                    $"https://{builtConfig["naos:secrets:vault:name"]}.vault.azure.net/",
                    builtConfig["naos:secrets:vault:clientId"],
                    builtConfig["naos:secrets:vault:clientSecret"],
                    new EnvironmentPrefixKeyVaultSecretManager());
            }

            return config;
        }
    }
}