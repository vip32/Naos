namespace Naos.FileStorage.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.File;
    using Naos.FileStorage.Domain;
    using Naos.Foundation;

    public static class Extensions
    {
        public static IEnumerable<CloudBlockBlob> Matches(this IEnumerable<CloudBlockBlob> blobs, Regex pattern)
        {
            return blobs.Where(blob => pattern?.IsMatch(blob.ToFileInfo().Path) != false);
        }

        public static IEnumerable<CloudFile> Matches(this IEnumerable<CloudFile> files, Regex pattern)
        {
            return files.Where(file => pattern?.IsMatch(file.ToFileInfo().Path) != false);
        }

        public static FileInformation ToFileInfo(this CloudBlockBlob blob)
        {
            if (blob.Properties.Length == -1)
            {
                return null;
            }

            var result = new FileInformation
            {
                Path = blob.Name,
                Name = blob.Name.SliceFromLast("/"),
                Size = blob.Properties.Length,
                Created = blob.Properties.Created?.UtcDateTime ?? DateTime.MinValue,
                Modified = blob.Properties.LastModified?.UtcDateTime ?? DateTime.MinValue,
            };

            result.Properties.Add(nameof(blob.Properties.ETag), blob.Properties.ETag);
            result.Properties.Add(nameof(blob.Properties.ContentType), blob.Properties.ContentType);

            return result;
        }

        public static FileInformation ToFileInfo(this CloudFile file)
        {
            if (file.Properties.Length == -1)
            {
                return null;
            }

            var result = new FileInformation
            {
                Path = file.Name,
                Name = file.Name.SliceFromLast("/"),
                Size = file.Properties.Length,
                //Created = file.Properties.Created?.UtcDateTime ?? DateTime.MinValue,
                Modified = file.Properties.LastModified?.UtcDateTime ?? DateTime.MinValue,
            };

            result.Properties.Add(nameof(file.Properties.ETag), file.Properties.ETag);
            result.Properties.Add(nameof(file.Properties.ContentType), file.Properties.ContentType);

            return result;
        }
    }
}