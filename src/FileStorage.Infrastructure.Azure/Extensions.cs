namespace Naos.Core.FileStorage.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Naos.Core.FileStorage.Domain;

    public static class Extensions
    {
        public static IEnumerable<CloudBlockBlob> Matches(this IEnumerable<CloudBlockBlob> blobs, Regex pattern)
        {
            return blobs.Where(blob => pattern == null || pattern.IsMatch(blob.ToFileInfo().Path));
        }

        public static FileInformation ToFileInfo(this CloudBlockBlob blob)
        {
            if (blob.Properties.Length == -1)
            {
                return null;
            }

            return new FileInformation
            {
                Path = blob.Name,
                Size = blob.Properties.Length,
                Created = blob.Properties.LastModified?.UtcDateTime ?? DateTime.MinValue,
                Modified = blob.Properties.LastModified?.UtcDateTime ?? DateTime.MinValue
            };
        }
    }
}