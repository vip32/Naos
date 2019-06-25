namespace Naos.Core.FileStorage.Csv.Domain
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.FileStorage.Domain;
    using Naos.Foundation;

    public static class FileStorageExtensions
    {
        public static Task<bool> SaveFileCsvAsync<T, THeader>(
            this IFileStorage storage,
            string path,
            T data,
            string itemSeperator = ";",
            CultureInfo cultureInfo = null,
            string dateTimeFormat = null,
            Dictionary<string, string> headersMap = null,
            CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            return storage.SaveFileObjectAsync(
                path,
                data,
                new CsvSerializer<THeader>(itemSeperator, cultureInfo, dateTimeFormat, headersMap),
                cancellationToken);
        }

        public static Task<bool> SaveFileCsvAsync<T>(
            this IFileStorage storage,
            string path,
            T data,
            string itemSeperator = ";",
            CultureInfo cultureInfo = null,
            string dateTimeFormat = null,
            CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            return storage.SaveFileObjectAsync(
                path,
                data,
                new CsvSerializer(itemSeperator, cultureInfo, dateTimeFormat),
                cancellationToken);
        }

        public static async Task<T> GetFileCsvAsync<T>(this IFileStorage storage, string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            return await storage.GetFileObjectAsync<T>(
                path,
                new CsvSerializer(),
                cancellationToken).AnyContext();
        }
    }
}
