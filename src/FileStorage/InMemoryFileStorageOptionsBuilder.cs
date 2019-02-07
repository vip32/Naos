namespace Naos.Core.FileStorage.Domain
{
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;

    public class InMemoryFileStorageOptionsBuilder :
        BaseOptionsBuilder<InMemoryFileStorageOptions, InMemoryFileStorageOptionsBuilder>
    {
        public InMemoryFileStorageOptionsBuilder Folder(long maxFileSize)
        {
            this.Target.MaxFileSize = maxFileSize;
            return this;
        }

        public InMemoryFileStorageOptionsBuilder Folder(int maxFiles)
        {
            this.Target.MaxFiles = maxFiles;
            return this;
        }

        public InMemoryFileStorageOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;
            return this;
        }
    }
}