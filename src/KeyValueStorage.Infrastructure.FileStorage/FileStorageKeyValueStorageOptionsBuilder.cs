namespace Naos.Core.KeyValueStorage.Infrastructure.FileStorage
{
    using Naos.Core.FileStorage.Domain;
    using Naos.Foundation;

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