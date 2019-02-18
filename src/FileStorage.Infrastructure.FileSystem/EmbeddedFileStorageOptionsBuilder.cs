namespace Naos.Core.FileStorage.Infrastructure
{
    using System.Reflection;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;

    public class EmbeddedFileStorageOptionsBuilder :
        BaseOptionsBuilder<EmbeddedFileStorageOptions, EmbeddedFileStorageOptionsBuilder>
    {
        public EmbeddedFileStorageOptionsBuilder Assembly(Assembly assembly)
        {
            this.Target.Assembly = assembly;
            return this;
        }

        public EmbeddedFileStorageOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;
            return this;
        }
    }
}