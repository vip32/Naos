namespace Naos.FileStorage
{
    using Naos.Foundation;

    public class InMemoryFileStorageOptions : OptionsBase
    {
        public long MaxFileSize { get; set; } = 1024 * 1024 * 256;

        public int MaxFiles { get; set; } = 1000;

        public ISerializer Serializer { get; set; }
    }
}
