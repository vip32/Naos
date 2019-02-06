namespace Naos.Core.FileStorage.Domain
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Common.Serialization;

    public interface IFileStorage : IDisposable
    {
        ISerializer Serializer { get; }

        Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default);

        Task<FileInformation> GetFileInformationAsync(string path);

        Task<bool> ExistsAsync(string path);

        Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default);

        Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default);

        Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default);

        Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default);

        Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default);

        Task<PagedResults> GetPagedFileListAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default);
    }
}
