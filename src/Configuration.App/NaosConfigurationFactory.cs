namespace Naos.Core.Configuration.App
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

        public static IConfigurationBuilder Extend(IConfigurationBuilder config, string[] args = null, string environmentName = "Development")
        {
            return CreateBuilder(null, args, config, environmentName);
        }

        public static IConfigurationBuilder Extend(IConfigurationBuilder config, string basePath, string[] args = null, string environmentName = "Development")
        {
            return CreateBuilder(basePath, args, config, environmentName);
        }

        public static IConfigurationRoot Create(string basePath = null, string[] args = null)
        {
            return CreateBuilder(basePath, args).Build();
        }

        private static IConfigurationBuilder CreateBuilder(string basePath = null, string[] args = null, IConfigurationBuilder builder = null, string environmentName = "Development")
        {
            builder = builder ?? new ConfigurationBuilder();
            builder.SetBasePath(basePath ?? AppDomain.CurrentDomain.BaseDirectory)
                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                  .AddEnvironmentVariables();
            builder.AddIf(!environmentName.IsNullOrEmpty(), b => b
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true));
            builder.AddIf(args != null, b => b
                .AddCommandLine(args));

            var configuration = builder.Build();
            builder.AddIf(!configuration["naos:secrets:userSecretsId"].IsNullOrEmpty(), b =>
                b.AddUserSecrets(configuration["naos:secrets:userSecretsId"])); // https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets
            configuration = builder.Build();
            builder.AddIf(configuration["naos:secrets:vault:enabled"].ToBool(true), b =>
            {
                if(configuration["naos:secrets:vault:name"].IsNullOrEmpty()
                    || configuration["naos:secrets:vault:clientId"].IsNullOrEmpty()
                    || configuration["naos:secrets:vault:clientSecret"].IsNullOrEmpty())
                {
                    throw new Exception("Naos Keyvault configuration provider cannot be used when secrets:keyvault:name, secrets:keyvault:clientId or secrets:keyvault:clientSecret are not provided by any of the configuration providers (json/env/args). Please make these configuration settings available or set secrets:keyvault:enabled to 'false'.");
                }

                b.AddAzureKeyVault(
                    $"https://{configuration["naos:secrets:vault:name"]}.vault.azure.net/",
                    configuration["naos:secrets:vault:clientId"],
                    configuration["naos:secrets:vault:clientSecret"],
                    //new CachedKeyVaultClient() // howto create new keyvault instance https://github.com/aspnet/Configuration/blob/master/src/Config.AzureKeyVault/AzureKeyVaultConfigurationExtensions.cs
                    new EnvironmentPrefixKeyVaultSecretManager());

                return b;
            });

            return builder;
        }
    }
}