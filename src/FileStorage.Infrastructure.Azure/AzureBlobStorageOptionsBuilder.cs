namespace Naos.Core.FileStorage.Infrastructure.Azure
{
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;

    public class AzureBlobStorageOptionsBuilder :
        BaseOptionsBuilder<AzureBlobStorageOptions, AzureBlobStorageOptionsBuilder>
    {
        public AzureBlobStorageOptionsBuilder ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString;
            return this;
        }

        public AzureBlobStorageOptionsBuilder ContainerName(string containerName)
        {
            this.Target.ContainerName = containerName;
            return this;
        }

        public AzureBlobStorageOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;
            return this;
        }
    }
}