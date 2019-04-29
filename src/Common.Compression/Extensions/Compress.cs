namespace Naos.Core.Common
{
    using System.IO;
    using EnsureThat;

    public static partial class CompressionExtensions
    {
        /// <summary>
        /// Compress the source stream into the destination stream.
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
    }
}
