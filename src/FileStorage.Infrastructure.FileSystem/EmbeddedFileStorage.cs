namespace Naos.Core.FileStorage.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.FileStorage.Domain;
    using Naos.Foundation;

    public class EmbeddedFileStorage : IFileStorage
    {
        private readonly object @lock = new object();
        private readonly EmbeddedFileStorageOptions options;

        public EmbeddedFileStorage(EmbeddedFileStorageOptions options)
        {
            this.options = options ?? new EmbeddedFileStorageOptions();
            this.options.Assemblies = this.options.Assemblies ?? new[] { Assembly.GetEntryAssembly() };
            this.Serializer = this.options.Serializer ?? DefaultSerializer.Create;
        }

        public EmbeddedFileStorage(Builder<EmbeddedFileStorageOptionsBuilder, EmbeddedFileStorageOptions> optionsBuilder)
            : this(optionsBuilder(new EmbeddedFileStorageOptionsBuilder()).Build())
        {
        }

        public ISerializer Serializer { get; }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            if(!await this.ExistsAsync(path))
            {
                return null;
            }

            var item = this.GetFileList(this.options.Assemblies)
                .FirstOrDefault(p => p.Path.SafeEquals(path));

            if(item == null)
            {
                return null;
            }

            foreach(var assembly in this.options.Assemblies)
            {
                if(item.Properties.GetValue<string>("assemblyName").SafeEquals(assembly.GetName().Name))
                {
                    return assembly.GetManifestResourceStream(item.Properties.GetValue<string>("resourceName"));
                }
            }

            return null;
        }

        public async Task<FileInformation> GetFileInformationAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            if(!await this.ExistsAsync(path))
            {
                return null;
            }

            return this.GetFileList(this.options.Assemblies)
                .FirstOrDefault(p => p.Path.SafeEquals(path));
        }

        public Task<bool> ExistsAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            return Task.FromResult(
                this.GetFileList(this.options.Assemblies).SafeAny(p => p.Path.SafeEquals(path)));
        }

        public Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResults> GetFileInformationsAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default)
        {
            searchPattern = PathHelper.Normalize(searchPattern);
            var result = this.GetFileList(this.options.Assemblies);

            if(result.IsNullOrEmpty())
            {
                return Task.FromResult(new PagedResults(null, false, null));
            }

            if(!searchPattern.IsNullOrEmpty())
            {
                result = result.Where(i => i.Path.EqualsPattern(searchPattern));
            }

            return Task.FromResult(
                new PagedResults(result.ToList().AsReadOnly()));
        }

        public void Dispose()
        {
        }

        private IEnumerable<FileInformation> GetFileList(IEnumerable<Assembly> assemblies)
        {
            var result = new List<FileInformation>();
            foreach(var assembly in assemblies.Safe())
            {
                var created = assembly.GetBuildDate();
                foreach(var resource in assembly.GetManifestResourceNames())
                {
                    var path = PathHelper.Normalize(resource.SliceTillLast(".").Replace(".", Path.DirectorySeparatorChar.ToString()) + "." + resource.SliceFromLast("."));
                    var fileInfo = new FileInformation
                    {
                        Path = path,
                        Name = path.SliceFromLast(Path.DirectorySeparatorChar.ToString()),
                        Created = created,
                        Modified = created
                    };
                    fileInfo.Properties.AddOrUpdate("resourceName", resource);
                    fileInfo.Properties.AddOrUpdate("assemblyName", assembly.GetName().Name);

                    result.Add(fileInfo);
                }
            }

            return result.OrderBy(f => f.Path);
        }
    }
}
