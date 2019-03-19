namespace Naos.Core.Infrastructure.Azure
{
    public class QueueStorageConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string ConnectionString { get; set; }

        public string Name { get; set; }
    }
}
