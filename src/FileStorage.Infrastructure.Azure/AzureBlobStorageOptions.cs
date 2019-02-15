namespace Naos.Core.FileStorage.Infrastructure.Azure
{
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;

    public class AzureBlobStorageOptions : BaseOptions
    {
        public string ConnectionString { get; set; }

        public string ContainerName { get; set; } = "storage";

        public ISerializer Serializer { get; set; }
    }
}