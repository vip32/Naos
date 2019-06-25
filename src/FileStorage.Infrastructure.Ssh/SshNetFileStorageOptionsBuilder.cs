namespace Naos.Core.FileStorage.Infrastructure
{
    using System;
    using System.IO;
    using System.Text;
    using Naos.Foundation;
    using Renci.SshNet;

    public class SshNetFileStorageOptionsBuilder :
        BaseOptionsBuilder<SshNetFileStorageOptions, SshNetFileStorageOptionsBuilder>
    {
        public SshNetFileStorageOptionsBuilder ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            return this;
        }

        public SshNetFileStorageOptionsBuilder Proxy(string proxy)
        {
            this.Target.Proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
            return this;
        }

        public SshNetFileStorageOptionsBuilder ProxyType(ProxyTypes proxyType)
        {
            this.Target.ProxyType = proxyType;
            return this;
        }

        public SshNetFileStorageOptionsBuilder PrivateKey(string privateKey)
        {
            EnsureThat.EnsureArg.IsNotNullOrEmpty(privateKey, nameof(privateKey));

            this.Target.PrivateKey = new MemoryStream(Encoding.UTF8.GetBytes(privateKey));
            return this;
        }

        public SshNetFileStorageOptionsBuilder PrivateKey(Stream privateKey)
        {
            this.Target.PrivateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
            return this;
        }

        public SshNetFileStorageOptionsBuilder PrivateKeyPassPhrase(string privateKeyPassPhrase)
        {
            this.Target.PrivateKeyPassPhrase = privateKeyPassPhrase ?? throw new ArgumentNullException(nameof(privateKeyPassPhrase));
            return this;
        }

        public SshNetFileStorageOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;
            return this;
        }
    }
}