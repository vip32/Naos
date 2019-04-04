namespace Naos.Core.Common
{
    using System.IO;
    using EnsureThat;

    public static partial class Extensions
    {
        public static byte[] ReadAllBytes(this Stream source)
        {
            if(source is MemoryStream)
            {
                return ((MemoryStream)source).ToArray();
            }

            using(var memoryStream = new MemoryStream())
            {
                source.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Compress the source stream into the destination stream
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void Compress(this Stream source, Stream destination)
        {
            EnsureArg.IsNotNull(destination, nameof(destination));

            if(source == null)
            {
                return;
            }

            CompressionHelper.Compress(source, destination);
        }

        /// <summary>
        /// Decompress source stream into the destination stream
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void Decompress(this Stream source, Stream destination)
        {
            EnsureArg.IsNotNull(destination, nameof(destination));

            if(source == null)
            {
                return;
            }

            CompressionHelper.Decompress(source, destination);
        }
    }
}
