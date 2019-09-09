namespace Naos.Core.FileStorage
{
    using Naos.Foundation;

    public class InMemoryFileStorageOptions : BaseOptions
    {
        public long MaxFileSize { get; set; } = 1024 * 1024 * 256;

        public int MaxFiles { get; set; } = 1000;

        public ISerializer Serializer { get; set; }
    }
}
