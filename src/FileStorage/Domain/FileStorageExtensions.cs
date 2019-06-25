namespace Naos.Core.FileStorage.Domain
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Foundation;

    public static class FileStorageExtensions
    {
        public static Task<bool> SaveFileObjectAsync<T>(this IFileStorage storage, string path, T data, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            var bytes = storage.Serializer.SerializeToBytes(data);
            return storage.SaveFileAsync(
                path,
                new MemoryStream(bytes),
                cancellationToken);
        }

        public static Task<bool> SaveFileObjectAsync<T>(this IFileStorage storage, string path, T data, ISerializer serializer, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(serializer, nameof(serializer));
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            var bytes = serializer.SerializeToBytes(data);
            return storage.SaveFileAsync(
                path,
                new MemoryStream(bytes),
                cancellationToken);
        }

        public static async Task<T> GetFileObjectAsync<T>(this IFileStorage storage, string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            using(var stream = await storage.GetFileStreamAsync(path, cancellationToken).AnyContext())
            {
                if(stream != null)
                {
                    return storage.Serializer.Deserialize<T>(stream);
                }
            }

            return default;
        }

        public static async Task<T> GetFileObjectAsync<T>(this IFileStorage storage, string path, ISerializer serializer, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNull(serializer, nameof(serializer));

            using(var stream = await storage.GetFileStreamAsync(path, cancellationToken).AnyContext())
            {
                if(stream != null)
                {
                    return serializer.Deserialize<T>(stream);
                }
            }

            return default;
        }

        public static Task<bool> SaveFileContentsAsync(this IFileStorage storage, string path, string contents, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            return storage.SaveFileAsync(
                path,
                new MemoryStream(Encoding.UTF8.GetBytes(contents ?? string.Empty)),
                cancellationToken);
        }

        public static async Task<string> GetFileContentsAsync(this IFileStorage storage, string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            using(var stream = await storage.GetFileStreamAsync(path, cancellationToken).AnyContext())
            {
                if(stream != null)
                {
                    return await new StreamReader(stream).ReadToEndAsync().AnyContext();
                }
            }

            return null;
        }

        public static async Task<byte[]> GetFileContentsRawAsync(this IFileStorage storage, string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            using(var stream = await storage.GetFileStreamAsync(path, cancellationToken).AnyContext())
            {
                if(stream == null)
                {
                    return null;
                }

                var buffer = new byte[16 * 1024];
                using(var ms = new MemoryStream())
                {
                    int read;
                    while((read = await stream.ReadAsync(buffer, 0, buffer.Length).AnyContext()) > 0)
                    {
                        await ms.WriteAsync(buffer, 0, read).AnyContext();
                    }

                    return ms.ToArray();
                }
            }
        }

        public static async Task<IReadOnlyCollection<FileInformation>> GetFileInformationsAsync(this IFileStorage storage, string searchPattern = null, int? limit = null, CancellationToken cancellationToken = default)
        {
            var files = new List<FileInformation>();
            var result = await storage.GetFileInformationsAsync(100, searchPattern, cancellationToken).AnyContext();
            do
            {
                foreach(var file in result.Files.Safe())
                {
                    files.Add(file);
                    if(limit.HasValue && limit.Value == files.Count)
                    {
                        return files;
                    }
                }
            }
            while(result.HasMore && await result.NextPageAsync().AnyContext());

            return files;
        }

        public static async Task DeleteFilesAsync(this IFileStorage storage, IEnumerable<FileInformation> files)
        {
            foreach(var file in files.Safe())
            {
                await storage.DeleteFileAsync(file.Path).AnyContext();
            }
        }
    }
}
