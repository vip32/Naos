namespace Naos.Core.FileStorage.Domain
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IFileStorage : IReadOnlyFileStorage
    {
        Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default);

        Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default);

        Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default);

        Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default);

        Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default);
    }
}
