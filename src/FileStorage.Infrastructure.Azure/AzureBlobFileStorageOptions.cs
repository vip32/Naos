namespace Naos.Core.FileStorage.Infrastructure
{
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;

    public class AzureBlobFileStorageOptions : BaseOptions
    {
        public string ConnectionString { get; set; }

        public string ContainerName { get; set; } = "storage";

        public ISerializer Serializer { get; set; }
    }
}