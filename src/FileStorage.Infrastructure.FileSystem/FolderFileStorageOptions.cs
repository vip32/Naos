namespace Naos.FileStorage.Infrastructure
{
    using Naos.Foundation;

    public class FolderFileStorageOptions : OptionsBase
    {
        public string Folder { get; set; }

        public ISerializer Serializer { get; set; }
    }
}
