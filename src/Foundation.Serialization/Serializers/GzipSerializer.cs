namespace Naos.Foundation
{
    using System;
    using System.IO;
    using System.IO.Compression;

    public class GzipSerializer : ISerializer
    {
        private readonly ISerializer inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="GzipSerializer"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public GzipSerializer(ISerializer inner)
        {
            this.inner = inner;
        }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="output">The output.</param>
        public void Serialize(object value, Stream output)
        {
            if (value == null)
            {
                return;
            }

            if (output == null)
            {
                return;
            }

            using (var compress = new DeflateStream(output, CompressionMode.Compress, true))
            {
                this.inner.Serialize(value, compress);
            }
        }

        /// <summary>
        /// Deserializes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="type">The type.</param>
        public object Deserialize(Stream input, Type type)
        {
            if (input == null)
            {
                return null;
            }

            input.Position = 0;
            using (var decompress = new DeflateStream(input, CompressionMode.Decompress, true))
            {
                return this.inner.Deserialize(decompress, type);
            }
        }

        /// <summary>
        /// Deserializes the specified input.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">The input.</param>
        public T Deserialize<T>(Stream input)
            where T : class
        {
            if (input == null)
            {
                return default;
            }

            input.Position = 0;
            using (var decompress = new DeflateStream(input, CompressionMode.Decompress, true))
            {
                return this.inner.Deserialize<T>(decompress);
            }
        }
    }
}
