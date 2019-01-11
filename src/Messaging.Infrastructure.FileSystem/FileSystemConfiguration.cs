namespace Naos.Core.Messaging.Infrastructure.FileSystem
{
    using System.IO;

    public class FileSystemConfiguration
    {
        public string Folder { get; set; } = Path.GetTempPath();

        public int ProcessDelay { get; set; } = 100;
    }
}
