namespace Naos.FileStorage.Infrastructure
{
    using System.Linq;
    using Naos.Foundation;

    public class AzureFileShareStorageOptionsBuilder :
        BaseOptionsBuilder<AzureFileShareStorageOptions, AzureFileShareStorageOptionsBuilder>
    {
        public AzureFileShareStorageOptionsBuilder ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString;
            return this;
        }

        public AzureFileShareStorageOptionsBuilder ShareName(string shareName)
        {
            this.Target.ShareName = shareName;
            return this;
        }

        public AzureFileShareStorageOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;

            return this;
        }
    }
}