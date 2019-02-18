namespace Naos.Core.FileStorage.Infrastructure
{
    using System.Reflection;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;

    public class EmbeddedFileStorageOptions : BaseOptions
    {
        public Assembly Assembly { get; set; }

        public ISerializer Serializer { get; set; }
    }
}
