namespace Naos.FileStorage.Infrastructure
{
    using Naos.Foundation;

    public class AzureFileShareStorageOptions : BaseOptions
    {
        public string ConnectionString { get; set; }

        public string ShareName { get; set; }

        public ISerializer Serializer { get; set; }
    }
}