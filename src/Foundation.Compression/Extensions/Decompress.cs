namespace Naos.Foundation
{
    using System.IO;
    using EnsureThat;

    public static partial class CompressionExtensions
    {
        /// <summary>
        /// Decompress source stream into the destination stream.
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
