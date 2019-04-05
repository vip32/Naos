namespace Naos.Core.KeyValueStorage.Infrastructure.FileStorage
{
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;

    public class FileStorageKeyValueStorageOptionsBuilder :
        BaseOptionsBuilder<FileStorageKeyValueStorageOptions, FileStorageKeyValueStorageOptionsBuilder>
    {
        public FileStorageKeyValueStorageOptionsBuilder FileStorage(IFileStorage fileStorage)
        {
            this.Target.FileStorage = fileStorage;
            return this;
        }
    }
}