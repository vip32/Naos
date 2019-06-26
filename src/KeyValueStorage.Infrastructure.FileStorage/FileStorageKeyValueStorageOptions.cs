namespace Naos.Core.KeyValueStorage.Infrastructure.FileStorage
{
    using Naos.Core.FileStorage.Domain;
    using Naos.Foundation;

    public class FileStorageKeyValueStorageOptions : BaseOptions
    {
        public IFileStorage FileStorage { get; set; }
    }
}
