namespace Naos.Core.FileStorage.Infrastructure
{
    using Naos.Core.Common;

    public class FolderFileStorageOptionsBuilder :
        BaseOptionsBuilder<FolderFileStorageOptions, FolderFileStorageOptionsBuilder>
    {
        public FolderFileStorageOptionsBuilder Folder(string folder)
        {
            this.Target.Folder = folder;
            return this;
        }

        public FolderFileStorageOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;
            return this;
        }
    }
}