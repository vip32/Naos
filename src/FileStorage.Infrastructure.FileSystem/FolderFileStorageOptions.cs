namespace Naos.Core.FileStorage.Infrastructure.FileSystem
{
    using Naos.Core.Common.Serialization;

    public class FolderFileStorageOptions
    {
        public string Folder { get; set; }

        public ISerializer Serializer { get; set; }
    }
}
