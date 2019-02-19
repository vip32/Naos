namespace Naos.Core.FileStorage.Domain
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Common.Serialization;

    public interface IReadOnlyFileStorage : IDisposable
    {
        ISerializer Serializer { get; }

        Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default);

        Task<FileInformation> GetFileInformationAsync(string path);

        Task<bool> ExistsAsync(string path);

        Task<PagedResults> GetFileInformationsAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default);
    }
}
