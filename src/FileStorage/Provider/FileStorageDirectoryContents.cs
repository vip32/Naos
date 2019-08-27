namespace Naos.Core.FileStorage
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using EnsureThat;
    using Microsoft.Extensions.FileProviders;
    using Naos.Core.FileStorage.Domain;
    using Naos.Foundation;

    public class FileStorageDirectoryContents : IDirectoryContents
    {
        private readonly IFileStorage fileStorage;
        private readonly string path;
        private readonly PagedResults pagedResults;

        public FileStorageDirectoryContents(IFileStorage fileStorage, string path)
        {
            EnsureArg.IsNotNull(fileStorage, nameof(fileStorage));

            this.fileStorage = fileStorage;
            this.path = path;
            this.pagedResults = !path.IsNullOrEmpty() ? this.fileStorage.GetFileInformationsAsync(9999, path.Contains("*") ? path : path + "*").Result : null;
        }

        public bool Exists => this.pagedResults?.Files.Safe().Any() == true;

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            foreach (var file in this.pagedResults?.Files.Safe().DistinctBy(f => f.Path))
            {
                yield return new FileStorageFileInfo(this.fileStorage, file.Path);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var file in this.pagedResults?.Files.Safe().DistinctBy(f => f.Path))
            {
                yield return new FileStorageFileInfo(this.fileStorage, file.Path);
            }
        }
    }
}
