namespace Naos.Core.FileStorage.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;

    public static class Extensions
    {
        public static IEnumerable<CloudBlockBlob> Matches(this IEnumerable<CloudBlockBlob> blobs, Regex pattern)
        {
            return blobs.Where(blob => pattern == null || pattern.IsMatch(blob.ToFileInfo().Path));
        }

        public static FileInformation ToFileInfo(this CloudBlockBlob blob)
        {
            if(blob.Properties.Length == -1)
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
    }
}