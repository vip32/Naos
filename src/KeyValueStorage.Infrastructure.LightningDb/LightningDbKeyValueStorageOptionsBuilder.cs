namespace Naos.KeyValueStorage.Infrastructure
{
    using Naos.Foundation;

    public class LightningDbKeyValueStorageOptionsBuilder :
        BaseOptionsBuilder<LightningDbKeyValueStorageOptions, LightningDbKeyValueStorageOptionsBuilder>
    {
        public LightningDbKeyValueStorageOptionsBuilder Folder(string folder)
        {
            this.Target.Folder = folder;
            return this;
        }
    }
}