namespace Naos.KeyValueStorage.Infrastructure
{
    using System.IO;
    using Naos.Foundation;

    public class LightningDbKeyValueStorageOptions : OptionsBase
    {
        public string Folder { get; set; } = Path.GetTempPath();
    }
}
