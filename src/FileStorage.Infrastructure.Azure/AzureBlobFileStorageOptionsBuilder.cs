namespace Naos.Core.FileStorage.Infrastructure
{
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;

    public class AzureBlobFileStorageOptionsBuilder :
        BaseOptionsBuilder<AzureBlobFileStorageOptions, AzureBlobFileStorageOptionsBuilder>
    {
        public AzureBlobFileStorageOptionsBuilder ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString;
            return this;
        }

        public AzureBlobFileStorageOptionsBuilder ContainerName(string containerName)
        {
            this.Target.ContainerName = containerName;
            return this;
        }

        public AzureBlobFileStorageOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;
            return this;
        }
    }
}