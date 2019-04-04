namespace Naos.Core.Common
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using EnsureThat;

    public static class CompressionHelper
    {
        private const int ZipLeadBytes = 0x04034b50;
        private const ushort GzipLeadBytes = 0x8b1f;

        public static byte[] Compress(byte[] source)
        {
            using(var sourceStream = new MemoryStream(source))
            {
                using(var destinationStream = new MemoryStream())
                {
                    StreamHelper.Compress(sourceStream, destinationStream);
                    return destinationStream.ToArray();
                }
            }
        }

        public static byte[] Decompress(byte[] source)
        {
            using(var sourceStream = new MemoryStream(source))
            {
                using(var destinationStream = new MemoryStream())
                {
                    StreamHelper.Decompress(sourceStream, destinationStream);
                    return destinationStream.ToArray();
                }
            }
        }

        public static bool IsGzipped(byte[] source)
        {
            if(source == null || source.Length < 2)
            {
                return false;
            }

            return BitConverter.ToUInt16(source, 0) == GzipLeadBytes;
        }

        public static bool IsPkZipped(byte[] source)
        {
            if(source == null || source.Length < 4)
            {
                return false;
            }

            return BitConverter.ToInt32(source, 0) == ZipLeadBytes;
        }
    }
}
