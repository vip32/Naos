namespace Naos.FileStorage.Infrastructure
{
    using System.Collections.Generic;
    using System.Reflection;
    using Naos.Foundation;

    public class EmbeddedFileStorageOptions : BaseOptions
    {
        public IEnumerable<Assembly> Assemblies { get; set; }

        public ISerializer Serializer { get; set; }
    }
}
