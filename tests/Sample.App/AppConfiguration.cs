namespace Naos.Sample.App
{
    using Naos.Core.App.Configuration;
    using Naos.Core.Infrastructure.Azure.CosmosDb;

    public class AppConfiguration : NaosAppConfiguration
    {
        public CosmosDbConfiguration CosmosDb { get; set; }
    }
}
