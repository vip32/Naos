namespace Naos.Core.FileStorage.Infrastructure
{
    using System.IO;
    using Naos.Core.Common;
    using Renci.SshNet;

    public class SshNetFileStorageOptions : BaseOptions
    {
        public string ConnectionString { get; set; }

        public string Proxy { get; set; }

        public ProxyTypes ProxyType { get; set; }

        public Stream PrivateKey { get; set; }

        public string PrivateKeyPassPhrase { get; set; }

        public ISerializer Serializer { get; set; }
    }
}
