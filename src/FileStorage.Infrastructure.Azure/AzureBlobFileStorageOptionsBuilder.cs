namespace Naos.Core.FileStorage.Infrastructure
{
    using System.Linq;
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
            this.Target.ContainerName =
                this.TidyContainerName(containerName.Default("default")).ToLower();
            return this;
        }

        public AzureBlobFileStorageOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;

            return this;
        }

        private string TidyContainerName(string name)
        {
            var removeChars = new[] { '*', '/', '#', '!', '?', '\\', '/', '|', '<', '>', '{', '}', '[', ']', '+', '=' };
            var filterChars = name.ToArray();

            return new string(filterChars
                     .Where(c => !removeChars.Contains(c))
                     .Select(c => c == ' ' ? '-' : c)
                     .Select(c => c == '_' ? '-' : c).ToArray());
        }
    }
}