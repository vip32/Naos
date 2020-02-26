namespace Naos.KeyValueStorage.Infrastructure.FileStorage
{
    using Naos.FileStorage.Domain;
    using Naos.Foundation;

    public class FileStorageKeyValueStorageOptions : OptionsBase
    {
        public IFileStorage FileStorage { get; set; }
    }
}
