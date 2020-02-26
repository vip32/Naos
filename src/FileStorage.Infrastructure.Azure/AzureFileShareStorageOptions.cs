namespace Naos.FileStorage.Infrastructure
{
    using Naos.Foundation;

    public class AzureFileShareStorageOptions : OptionsBase
    {
        public string ConnectionString { get; set; }

        public string ShareName { get; set; }

        public ISerializer Serializer { get; set; }
    }
}