namespace Naos.Core.Common
{
    using System.IO;
    using System.IO.Compression;
    using EnsureThat;

    public static class StreamHelper
    {
        public static void Compress(Stream source, Stream destination)
        {
            EnsureArg.IsNotNull(destination, nameof(destination));

            if(source == null)
            {
                return;
            }

            using(var compressor = new GZipStream(destination, CompressionLevel.Optimal, true))
            {
                source.CopyTo(compressor);
                compressor.Flush();
            }
        }

        public static void Decompress(Stream source, Stream destination)
        {
            EnsureArg.IsNotNull(destination, nameof(destination));

            if(source == null)
            {
                return;
            }

            using(var decompressor = new GZipStream(source, CompressionMode.Decompress, true))
            {
                decompressor.CopyTo(destination);
                destination.Flush();
            }
        }

        public static string FromStream(Stream stream)
        {
            if(stream == null)
            {
                return null;
            }

            stream.Position = 0;
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        ///// <summary>
        /////     Stream to object T
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="stream">The stream.</param>
        ///// <returns></returns>
        // public static T fromstream<T>(Stream stream)
        // {
        //    if (stream == null)
        //    {
        //        return default(T);
        //    }

        // stream.Seek(0, SeekOrigin.Begin);
        //    var serializer = new XmlSerializer(typeof(T));
        //    return (T)serializer.deserialize(stream);
        // }
        public static Stream ToStream(string value)
        {
            if(value == null)
            {
                return null;
            }

            var stream = new MemoryStream();
            var sw = new StreamWriter(stream);
            sw.Write(value);
            sw.Flush();

            stream.Position = 0;
            return stream;
        }

        public static byte[] ToBytes(Stream stream)
        {
            if(stream == null)
            {
                return null;
            }

            var buffer = new byte[16 * 1024];
            using(var ms = new MemoryStream())
            {
                int read;
                while((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
    }
}