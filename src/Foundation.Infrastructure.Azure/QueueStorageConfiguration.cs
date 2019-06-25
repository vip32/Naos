namespace Naos.Foundation.Infrastructure
{
    public class QueueStorageConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string ConnectionString { get; set; }

        public string Name { get; set; }
    }
}
