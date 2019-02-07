namespace Naos.Core.FileStorage.Infrastructure.Azure
{
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;

    public class AzureFileStorageOptionsBuilder : OptionsBuilder<AzureFileStorageOptions>
    {
        public AzureFileStorageOptionsBuilder ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString;
            return this;
        }

        public AzureFileStorageOptionsBuilder ContainerName(string containerName)
        {
            this.Target.ContainerName = containerName;
            return this;
        }

        public AzureFileStorageOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;
            return this;
        }
    }
}