namespace Naos.KeyValueStorage.Infrastructure
{
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