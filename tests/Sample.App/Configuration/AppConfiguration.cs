namespace Naos.Core.Sample.App
{
    using Naos.Core.App.Configuration;

    public class AppConfiguration
    {
        public string Name { get; set; }

        public CosmosDbConfiguration CosmosDb { get; set; }
    }
}
