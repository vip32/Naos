namespace Naos.Core.FileStorage.Infrastructure.FileSystem
{
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;

    public class FolderFileStorageOptionsBuilder : OptionsBuilder<FolderFileStorageOptions>
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