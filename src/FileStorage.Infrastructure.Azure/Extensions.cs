namespace Naos.Core.FileStorage.Infrastructure.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Naos.Core.FileStorage.Domain;

    public static class Extensions
    {
        public static IEnumerable<CloudBlockBlob> MatchesPattern(this IEnumerable<CloudBlockBlob> blobs, Regex patternRegex)
        {
            return blobs.Where(blob => patternRegex == null || patternRegex.IsMatch(blob.ToFileInfo().Path));
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