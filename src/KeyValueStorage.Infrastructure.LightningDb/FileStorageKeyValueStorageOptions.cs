namespace Naos.KeyValueStorage.Infrastructure
{
    using Naos.Foundation;

    public class FileStorageKeyValueStorageOptions : OptionsBase
    {
        public IFileStorage FileStorage { get; set; }
    }
}
