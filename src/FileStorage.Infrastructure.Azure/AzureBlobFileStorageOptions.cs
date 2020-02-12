namespace Naos.FileStorage.Infrastructure
{
    using Naos.Foundation;

    public class AzureBlobFileStorageOptions : OptionsBase
    {
        public string ConnectionString { get; set; }

        public string ContainerName { get; set; } = "storage";

        public ISerializer Serializer { get; set; }
    }
}