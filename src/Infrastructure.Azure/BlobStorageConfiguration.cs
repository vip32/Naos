namespace Naos.Core.Infrastructure.Azure
{
    public class BlobStorageConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string ConnectionString { get; set; }

        public string ContainerName { get; set; }

        public string File { get; set; }

        public string Folder { get; set; }
    }
}
