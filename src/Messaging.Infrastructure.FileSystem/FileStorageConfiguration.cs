namespace Naos.Messaging.Infrastructure
{
    public class FileStorageConfiguration
    {
        public bool Enabled { get; set; }

        public string Folder { get; set; }

        public int ProcessDelay { get; set; }
    }
}
