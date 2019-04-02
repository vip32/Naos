namespace Naos.Core.FileStorage.Infrastructure
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;

    public class AzureKeyVaultFileStorage : IFileStorage
    {
        // TODO: https://github.com/aloneguid/storage/blob/master/src/Azure/Storage.Net.Microsoft.Azure.KeyVault/Blob/AzureKeyVaultBlobStorageProvider.cs
        public ISerializer Serializer => throw new System.NotImplementedException();

        public Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> ExistsAsync(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task<FileInformation> GetFileInformationAsync(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task<PagedResults> GetFileInformationsAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}