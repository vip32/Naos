namespace Naos.FileStorage.Infrastructure
{
    using System.Collections.Generic;
    using System.Reflection;
    using Naos.Foundation;

    public class EmbeddedFileStorageOptionsBuilder :
        BaseOptionsBuilder<EmbeddedFileStorageOptions, EmbeddedFileStorageOptionsBuilder>
    {
        public EmbeddedFileStorageOptionsBuilder Assembly(IEnumerable<Assembly> assemblies)
        {
            this.Target.Assemblies = assemblies;
            return this;
        }

        public EmbeddedFileStorageOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;
            return this;
        }
    }
}