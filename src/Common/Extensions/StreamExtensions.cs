namespace Naos.Core.Common
{
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using EnsureThat;

    public static partial class Extensions
    {
        public static byte[] ReadAllBytes(this Stream source)
        {
            if(source == null)
            {
                return default;
            }

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
        /// Detects the text encoding for the given <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The stream to detect the encoding for.</param>
        /// <param name="default">Default encoding if none found. </param>
        [DebuggerStepThrough]
        public static Encoding DetectEncoding(this Stream source, Encoding @default)
        {
            EnsureArg.IsNotNull(source, nameof(source));

            var pos = source.Position;
            try
            {
                using(var reader = new StreamReader(source, @default, true, 1, true))
                {
                    var next = reader.Peek();
                    return reader.CurrentEncoding;
                }
            }
            finally
            {
                if(source.CanSeek)
                {
                    // reset position
                    source.Position = pos;
                }
            }
        }
    }
}
