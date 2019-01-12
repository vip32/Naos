namespace Naos.Core.Discovery.App
{
    using System.IO;

    public class FileSystemServiceRegistryConfiguration
    {
        public string Folder { get; set; } = Path.GetTempPath();
    }
}
