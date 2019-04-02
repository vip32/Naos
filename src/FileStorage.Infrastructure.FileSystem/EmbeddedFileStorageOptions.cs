namespace Naos.Core.FileStorage.Infrastructure
{
    using System.Collections.Generic;
    using System.Reflection;
    using Naos.Core.Common;

    public class EmbeddedFileStorageOptions : BaseOptions
    {
        public IEnumerable<Assembly> Assemblies { get; set; }

        public ISerializer Serializer { get; set; }
    }
}
