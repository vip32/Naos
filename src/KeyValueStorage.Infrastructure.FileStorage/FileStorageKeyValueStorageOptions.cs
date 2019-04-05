namespace Naos.Core.KeyValueStorage.Infrastructure.FileStorage
{
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;

    public class FileStorageKeyValueStorageOptions : BaseOptions
    {
        public IFileStorage FileStorage { get; set; }
    }
}
