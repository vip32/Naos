namespace Naos.Core.FileStorage.Infrastructure
{
    using Naos.Core.Common;

    public class FolderFileStorageOptions : BaseOptions
    {
        public string Folder { get; set; }

        public ISerializer Serializer { get; set; }
    }
}
