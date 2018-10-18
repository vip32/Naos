namespace Naos.Core.Sample.App
{
    using Naos.Core.App.Configuration;

    public class SampleAppconfiguration : NaosConfiguration
    {
        public string AppName { get; set; }

        public CosmosDbConfiguration CosmosDb { get; set; }
    }
}
