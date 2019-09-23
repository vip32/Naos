namespace Naos.FileStorage
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Primitives;
    using Naos.FileStorage.Domain;

    public class FileStorageProvider : IFileProvider, IDisposable
    {
        // inspiration https://github.com/evorine/S3FileProvider
        private readonly IFileStorage fileStorage;

        public FileStorageProvider(IFileStorage fileStorage)
        {
            EnsureArg.IsNotNull(fileStorage, nameof(fileStorage));

            this.fileStorage = fileStorage;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            throw new NotImplementedException();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return new FileStorageFileInfo(this.fileStorage, subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }

        public void Dispose()
        {
            this.fileStorage?.Dispose();
        }
    }
}
